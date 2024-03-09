using UnityEngine;
using Unity.Services.Lobbies.Models;
using Battle.Board;
using VersusMode;




#if !DISABLESTEAMWORKS
using Mirror;
using UnityEngine.SceneManagement;
using Steamworks;
#endif

namespace Networking {
    public class SteamLobbyManager : MonoBehaviour {

        public static SteamLobbyManager instance {get; private set;}

        #if !DISABLESTEAMWORKS
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<LobbyMatchList_t> lobbyListReceived;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<PersonaStateChange_t> personaChanged;
        private const string HostAddressKey = "HostAddress";
        public bool steamInitialized {get; private set;} = false;
        public CSteamID lobbyId {get; private set;}
        #endif

        private void Start() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(this);
            InitializeSteam();
        }

        public void CreateLobby() {
            #if !DISABLESTEAMWORKS
                Debug.Log("Starting steam lobby");
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NetworkManager.singleton.maxConnections);
            #else
                Debug.LogError("Trying to start a Steam lobby, but Steamworks is disabled!");
            #endif
        }

        public void JoinLobbyWithID(string idStr) {
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

        public void OnDisconnected() {
            SteamMatchmaking.LeaveLobby(lobbyId);
        }

        #if !DISABLESTEAMWORKS
        public void InitializeSteam() {
            if (steamInitialized) {
                Debug.LogWarning("SteamManager already initialized");
                return;
            }
            
            if (!SteamManager.Initialized) {
                Debug.LogError("SteamManager is not initialized!");
                return;
            }

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyListReceived = Callback<LobbyMatchList_t>.Create(OnLobbyMatchListReceived);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            personaChanged = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

            steamInitialized = true;
        }

        private void RequestUserData(CSteamID id) {
            bool needsFetch = SteamFriends.RequestUserInformation(id, bRequireNameOnly: true); // eventually need both username and avatar
            if (!needsFetch) HandleUserData(id);
        }

        private void OnPersonaStateChanged(PersonaStateChange_t callback) {
            CSteamID id = new CSteamID(callback.m_ulSteamID);
            HandleUserData(id);
        }

        private void HandleUserData(CSteamID id) {
            // set data accordingly 
            if (id == SteamUser.GetSteamID()) {
                NetPlayer player = CharSelectMenu.Instance.p1Selector.netPlayer;
                if (player) player.SetUsername(SteamFriends.GetPersonaName());
            } else {
                NetPlayer player = CharSelectMenu.Instance.p2Selector.netPlayer;
                if (player) player.SetUsername(SteamFriends.GetFriendPersonaName(id));
            }
        }

        private void OnLobbyCreated(LobbyCreated_t callback) {
            if (callback.m_eResult != EResult.k_EResultOK) {
                Debug.LogError("Error creating steam lobby: "+callback.m_eResult);
                OnlineMenu.singleton.ShowOnlineMenu();
                return;
            }

            NetworkManager.singleton.StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString()+"'s lobby");
        }

        private void OnLobbyMatchListReceived(LobbyMatchList_t callback) {
            for (int i = 0; i < callback.m_nLobbiesMatching; i++) {
                var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                if (!lobbyId.IsValid() || !lobbyId.IsLobby()) continue;
            }
        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
            Debug.Log("Steam lobby join requested!");

            // if joining from steam, make sure the charselect scene is loaded first before joining lobby
            Storage.online = true; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true;
            Storage.level = null;
            if (SceneManager.GetActiveScene().name != "CharSelect") {
                SceneManager.LoadScene("CharSelect");
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }


        private void OnLobbyEntered(LobbyEnter_t callback) {
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

                CSteamID opponentId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, 0);
                RequestUserData(opponentId);
            }
        }
        #endif
    }
}