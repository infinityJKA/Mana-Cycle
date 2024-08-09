using UnityEngine;
using System.Collections.Generic;


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
        
        protected static Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected static Callback<LobbyEnter_t> lobbyEntered;
        protected static Callback<PersonaStateChange_t> personaChanged;
        private const string HostAddressKey = "HostAddress";


        protected static Callback<AvatarImageLoaded_t> avatarImageLoaded; 

        // lobby search callbacks
        protected static Callback<LobbyMatchList_t> lobbyListReceived;
        protected static Callback<LobbyDataUpdate_t> lobbyDataUpdated;

        // list of all CSteamIDs of (public) discovered lobbies.
        private static List<CSteamID> lobbyIDs;

        // ID of the current lobby that has been entered.
        public static CSteamID currentLobbyId {get; private set;}

        // Maps Steam IDs to players within the current lobby.
        public static Dictionary<CSteamID, NetPlayer> netPlayersByID;

        static SteamLobbyManager() {
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyListReceived = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
            lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            personaChanged = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarLoaded);
            Debug.Log("SteamLobbyManager listening for callbacks!");

            lobbyIDs = new List<CSteamID>();
            netPlayersByID = new Dictionary<CSteamID, NetPlayer>();
        }
        #endif

        public static void CreateLobby() {
            #if !DISABLESTEAMWORKS
                Debug.Log("Starting steam lobby");
                var lobbyType = OnlineMenu.singleton.publicToggle.isOn ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypePrivate;
                SteamMatchmaking.CreateLobby(lobbyType, 2);
            #else
                Debug.LogError("Trying to start a Steam lobby, but Steamworks is disabled!");
            #endif
        }

        public static void JoinLobbyWithStringId(string idStr) {
            #if !DISABLESTEAMWORKS
                ulong id;
                if (!ulong.TryParse(idStr, out id)) {
                    throw new System.Exception("Enter a valid join code");
                }
                Debug.Log("Joining steam lobby");

                // SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
                // SteamMatchmaking.RequestLobbyList();

                JoinLobby(id);
            #else
                Debug.LogError("Trying to join a Steam lobby, but Steamworks is disabled!");
            #endif
        }

        public static void JoinLobby(ulong lobbyID) {
            #if !DISABLESTEAMWORKS
                SteamMatchmaking.JoinLobby((CSteamID)lobbyID);
            #else
                Debug.LogError("Cannot join lobby; steam not enabled!");
            #endif
        }

        #if !DISABLESTEAMWORKS
        public static void DisconnectFromLobby() {
            SteamMatchmaking.LeaveLobby(currentLobbyId);
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

        // lobbies searching
        public static void GetLobbiesList() {
            if (lobbyIDs.Count > 0) lobbyIDs.Clear();

            SteamMatchmaking.AddRequestLobbyListResultCountFilter(10);
            SteamMatchmaking.RequestLobbyList();
        }

        private static void OnLobbyCreated(LobbyCreated_t callback) {
            if (callback.m_eResult != EResult.k_EResultOK) {
                PopupManager.instance.ShowErrorMessage("Error creating Steam lobby: "+callback.m_eResult);
                Debug.LogError("Error creating Steam lobby: "+callback.m_eResult);
                OnlineMenu.singleton.ShowOnlineMenu();
                return;
            }

            NetworkManager.singleton.StartHost();

            var lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(lobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(lobbyID, "name", SteamFriends.GetPersonaName().ToString()+"'s lobby");

            Debug.Log("lobby created! id: "+callback.m_ulSteamIDLobby);
        }

        public static void OpenFriendsList() {
            SteamFriends.ActivateGameOverlay("friends");
        }

        public static void OpenInviteDialog() {
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyId);
        }

        private static void OnGetLobbyList(LobbyMatchList_t callback) {
            Debug.Log("Lobby list received.");
            LobbyListManager.instance.DestroyLobbyEntries();

            if (callback.m_nLobbiesMatching == 0) {
                LobbyListManager.instance.NoLobbiesAvailable();
            }

            for (int i = 0; i < callback.m_nLobbiesMatching; i++) {
                CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                // if (!lobbyID.IsValid() || !lobbyID.IsLobby()) continue;
                lobbyIDs.Add(lobbyID);
                SteamMatchmaking.RequestLobbyData(lobbyID);
            }
        }

        private static void OnGetLobbyData(LobbyDataUpdate_t callback) {
            string name = SteamMatchmaking.GetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "name");
            LobbyListManager.instance.DisplayLobby(callback.m_ulSteamIDLobby, name);
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
            currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);

            if (NetworkClient.activeHost) {
                CSteamID opponentId = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyId, 1);
                RequestUserData(opponentId);

                // open invite dialog right away when host started to invite friend.
                // may remove this in future
                // but this same menu can also be reached easily by pressing tab (or shift_tab to open full steam overlay, friends menu included)
                OpenInviteDialog();
            }
            else if (!NetworkServer.active)
            {
                string hostAddress = SteamMatchmaking.GetLobbyData(currentLobbyId, HostAddressKey);
                NetworkManager.singleton.networkAddress = hostAddress;
                NetworkManager.singleton.StartClient();

                CSteamID hostId = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyId, 0);
                RequestUserData(hostId);
            }
        }

        public static ulong GetLobbyMemberID(int index) {
            return (ulong)SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyId, index);
        }

        public static void AddNetPlayerForID(ulong id, NetPlayer player) {
            netPlayersByID.TryAdd((CSteamID)id, player);
        }

        public static void LoadAvatar(ulong id) {
            int imageID = SteamFriends.GetLargeFriendAvatar((CSteamID)id); // will call OnAvatarLoaded callback when loading finished
            if (imageID == -1) {
                Debug.LogError("Trying to load invalid avatar image ID");
                return;
            }
            Debug.Log("loading avatar for id "+id);
        }

        private static void OnAvatarLoaded(AvatarImageLoaded_t callback) {
            if (netPlayersByID.TryGetValue(callback.m_steamID, out NetPlayer netPlayer)) {
                var texture = GetSteamImageAsTexture(callback.m_iImage);
                netPlayer.SetAvatar(texture);
                Debug.Log("received avatar for id "+callback.m_steamID);
            } else {
                Debug.LogError("Received avatar, but no NetPlayer with id "+callback.m_steamID);
            }
        }

        private static Texture2D GetSteamImageAsTexture(int iImage) {
            Texture2D texture = null;

            bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
            if (isValid)
            {
                byte[] image = new byte[width * height * 4];

                isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

                if (isValid)
                {
                    texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                }
            }

            return texture;
        }

        #endif
    }
}