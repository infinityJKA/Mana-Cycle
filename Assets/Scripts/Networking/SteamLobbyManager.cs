using UnityEngine;

#if !DISABLESTEAMWORKS
using Mirror;
using UnityEngine.SceneManagement;
using Steamworks;
#endif

namespace Networking {
    public class SteamLobbyManager {

        #if !DISABLESTEAMWORKS
        protected static Callback<LobbyCreated_t> lobbyCreated;
        protected static Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected static Callback<LobbyEnter_t> lobbyEntered;
        private const string HostAddressKey = "HostAddress";
        public static bool steamInitialized {get; private set;} = false;
        #endif

        #if !DISABLESTEAMWORKS
        public static void InitializeSteam() {
            if (NetworkManagerManager.networkManagerType != NetworkManagerManager.NetworkManagerType.Steam) {
                Debug.LogWarning("Trying to use steam, but it is disabled");
            }

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
        #endif

        public static void CreateLobby() {
            #if !DISABLESTEAMWORKS
                Debug.Log("Starting steam lobby");
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NetworkManager.singleton.maxConnections);
            #else
                Debug.LogError("Trying to start a Steam lobby, but Steamworks is disabled!");
            #endif
        }

        #if !DISABLESTEAMWORKS
        private static void OnLobbyCreated(LobbyCreated_t callback) {
            if (callback.m_eResult != EResult.k_EResultOK) {
                Debug.LogError("Error creating steam lobby "+callback.m_eResult);
                OnlineMenu.singleton.ShowOnlineMenu();
                return;
            }

            NetworkManager.singleton.StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        }

        private static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
            Debug.Log("Steam lobby join requested!");
            // if joining from steam, make sure the charselect scene is loaded first before joining lobby
            if (SceneManager.GetActiveScene().name != "CharSelect") {
                Storage.online = true; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true;
                SceneManager.LoadScene("CharSelect");
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private static void OnLobbyEntered(LobbyEnter_t callback) {
            if (NetworkServer.active) return; // do not run on host

            string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

            NetworkManager.singleton.networkAddress = hostAddress;
            NetworkManager.singleton.StartClient();
        }
        #endif
    }
}