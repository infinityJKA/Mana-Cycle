using UnityEngine;

#if !DISABLESTEAMWORKS
using Unity.Services.Lobbies.Models;
using Battle.Board;
using VersusMode;
using Steamworks;
using Mirror;
using UnityEngine.SceneManagement;
#endif

namespace Networking {
    public class SteamLobbyManager {

        #if !DISABLESTEAMWORKS
        protected static Callback<LobbyCreated_t> lobbyCreated;
        protected static Callback<LobbyMatchList_t> lobbyListReceived;
        protected static Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected static Callback<LobbyEnter_t> lobbyEntered;
        protected static Callback<PersonaStateChange_t> personaChanged;
        private const string HostAddressKey = "HostAddress";
        public static CSteamID lobbyId {get; private set;}  

        static SteamLobbyManager() {
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyListReceived = Callback<LobbyMatchList_t>.Create(OnLobbyMatchListReceived);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            personaChanged = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
        #endif

        public static void CreateLobby() {
            #if !DISABLESTEAMWORKS
                Debug.Log("Starting steam lobby");
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NetworkManager.singleton.maxConnections);
            #else
                Debug.LogError("Trying to start a Steam lobby, but Steamworks is disabled!");
            #endif
        }

        public static void JoinLobbyWithID(string idStr) {
            #if !DISABLESTEAMWORKS
                ulong id;
                if (!ulong.TryParse(idStr, out id)) {
                    throw new System.Exception("Enter a valid join code");
                }
                Debug.Log("Joining steam lobby");

                // SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
                // SteamMatchmaking.RequestLobbyList();

                SteamMatchmaking.JoinLobby(new CSteamID(id));
            #else
                Debug.LogError("Trying to join a Steam lobby, but Steamworks is disabled!");
            #endif
        }

        #if !DISABLESTEAMWORKS
        public static void OnDisconnected() {
            SteamMatchmaking.LeaveLobby(lobbyId);
        }
        private void OnDestroy() {
            SteamAPI.Shutdown();
        }

        private static void RequestUserData(CSteamID id) {
            bool needsFetch = SteamFriends.RequestUserInformation(id, bRequireNameOnly: true); // eventually need both username and avatar
            if (!needsFetch) HandleUserData(id);
        }

        private static void OnPersonaStateChanged(PersonaStateChange_t callback) {
            CSteamID id = new CSteamID(callback.m_ulSteamID);
            HandleUserData(id);
        }

        private static void HandleUserData(CSteamID id) {
            if (SceneManager.GetActiveScene().name != "CharSelect") return;

            // set data accordingly 
            if (id == SteamUser.GetSteamID()) {
                NetPlayer player = CharSelectMenu.Instance.p1Selector.netPlayer;
                if (player) player.SetUsername(SteamFriends.GetPersonaName());
            } else {
                NetPlayer player = CharSelectMenu.Instance.p2Selector.netPlayer;
                if (player) player.SetUsername(SteamFriends.GetFriendPersonaName(id));
            }
        }

        private static void OnLobbyCreated(LobbyCreated_t callback) {
            if (callback.m_eResult != EResult.k_EResultOK) {
                Debug.LogError("Error creating steam lobby: "+callback.m_eResult);
                OnlineMenu.singleton.ShowOnlineMenu();
                return;
            }

            NetworkManager.singleton.StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString()+"'s lobby");
        }

        private static void OnLobbyMatchListReceived(LobbyMatchList_t callback) {
            for (int i = 0; i < callback.m_nLobbiesMatching; i++) {
                var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                if (!lobbyId.IsValid() || !lobbyId.IsLobby()) continue;
            }
        }

        private static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
            Debug.Log("Steam lobby join requested!");

            // if joining from steam, make sure the charselect scene is loaded first before joining lobby
            Storage.online = true; 
            Storage.isPlayerControlled1 = true; 
            Storage.isPlayerControlled2 = true;
            Storage.level = null;
            Storage.gamemode = Storage.GameMode.Versus;
            if (SceneManager.GetActiveScene().name != "CharSelect") {
                TransitionScript.instance.onTransitionOut = () => SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
                TransitionScript.instance.WipeToScene("CharSelect");
            } else {
                SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
            }
        }


        private static void OnLobbyEntered(LobbyEnter_t callback) {
            lobbyId = new CSteamID(callback.m_ulSteamIDLobby);

            if (NetworkClient.activeHost) {
                CSteamID opponentId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, 1);
                RequestUserData(opponentId);
            }
            else if (!NetworkServer.active)
            {
                string hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey);
                NetworkManager.singleton.networkAddress = hostAddress;
                NetworkManager.singleton.StartClient();

                CSteamID hostId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, 0);
                RequestUserData(hostId);
            }
        }
        #endif
    }
}