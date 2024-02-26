using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class LobbyManager {

    protected static Callback<LobbyCreated_t> lobbyCreated;
    protected static Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected static Callback<LobbyEnter_t> lobbyEntered;

    static bool usingSteam = false;
    private const string HostAddressKey = "HostAddress";

    static LobbyManager() {
        if (usingSteam && !SteamManager.Initialized) {
            Debug.LogWarning("SteamManager is not initialized");
            usingSteam = false;
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public static void CreateLobby() {
        if (usingSteam) {
            Debug.LogWarning("Starting steam lobby");
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NetworkManager.singleton.maxConnections);
        } else {
            NetworkManager.singleton.StartHost();
        }
    }

    private static void OnLobbyCreated(LobbyCreated_t callback) {
        if (callback.m_eResult != EResult.k_EResultOK) {
            Debug.LogError("Error creating lobby "+callback.m_eResult);
            OnlineMenu.singleton.ShowOnlineMenu();
            return;
        }

        NetworkManager.singleton.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    private static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
        Debug.LogWarning("Lobby join requested!");
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
}