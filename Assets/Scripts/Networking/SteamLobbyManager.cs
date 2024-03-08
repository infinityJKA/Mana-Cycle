using UnityEngine;

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
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> lobbyEntered;
        private const string HostAddressKey = "HostAddress";
        public bool steamInitialized {get; private set;} = false;
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
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

            steamInitialized = true;
        }

        private void OnLobbyCreated(LobbyCreated_t callback) {
            if (callback.m_eResult != EResult.k_EResultOK) {
                Debug.LogError("Error creating steam lobby: "+callback.m_eResult);
                OnlineMenu.singleton.ShowOnlineMenu();
                return;
            }

            NetworkManager.singleton.StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString());
        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
            Debug.Log("Steam lobby join requested!");
            // if joining from steam, make sure the charselect scene is loaded first before joining lobby
            if (SceneManager.GetActiveScene().name != "CharSelect") {
                Storage.online = true; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true;
                SceneManager.LoadScene("CharSelect");
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t callback) {
            if (NetworkServer.active) return; // do not run on host

            string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

            if (NetworkServer.active) return;

            NetworkManager.singleton.networkAddress = hostAddress;
            NetworkManager.singleton.StartClient();
        }
        #endif
    }
}