using Networking;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

using TMPro;
using UnityEngine;

public class LobbyEntry : MonoBehaviour {
    private ulong lobbyId;
    private string lobbyName;
    [SerializeField] private TMP_Text lobbyNameText;

    public void SetLobbyData(ulong id, string name) {
        lobbyId = id;
        lobbyName = name;

        lobbyNameText.text = lobbyName;
    }

    public void JoinLobbyPressed() {
        #if !DISABLESTEAMWORKS
        SteamLobbyManager.JoinLobby(lobbyId);
        #else
        Debug.LogError("Steam not enabled! Cannot join lobby");
        #endif
    }
}