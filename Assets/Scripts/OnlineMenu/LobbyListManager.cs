using System.Collections.Generic;
using Networking;
using Steamworks;
using TMPro;
using UnityEngine;

public class LobbyListManager : MonoBehaviour {
    [SerializeField] private GameObject lobbyEntryPrefab;
    [SerializeField] private Transform lobbyListContentTransform;

    [SerializeField] private TMP_Text noLobbiesLabel;

    // key: lobby id. value: lobby entry representing that lobby
    private Dictionary<ulong, LobbyEntry> lobbyEntries;

    public static LobbyListManager instance;

    private void Awake() {
        instance = this;
        lobbyEntries = new Dictionary<ulong, LobbyEntry>();

        foreach (Transform child in lobbyListContentTransform) {
            Destroy(child.gameObject);
        }
    }

    private void OnEnable() {
        DestroyLobbyEntries();
        noLobbiesLabel.text = "Loading lobbies...";
        SteamLobbyManager.GetLobbiesList();
    }

    public void LobbiesCountFound(int count) {
        noLobbiesLabel.text = "Found "+count+" lobbies...";
    }

    public void LobbiesLoadError() {
        noLobbiesLabel.text = "Error loading lobbies";
        noLobbiesLabel.color = Color.red;
    }

    public void NoLobbiesAvailable() {
        noLobbiesLabel.text = "No public lobbies :(";
        noLobbiesLabel.color = Color.white;
    }

    public void DisplayLobby(ulong id, string name) {
        if (gameObject.activeInHierarchy) return;

        if (lobbyEntries.ContainsKey(id)) {
            lobbyEntries[id].SetLobbyData(id, name);
            return;
        }

        GameObject createdItem = Instantiate(lobbyEntryPrefab);
        var lobbyEntry = createdItem.GetComponent<LobbyEntry>();
        lobbyEntry.SetLobbyData(id, name);
        createdItem.transform.SetParent(lobbyListContentTransform, false);
        lobbyEntries.Add(id, lobbyEntry);
    }

    public void DestroyLobbyEntries() {
        foreach (var lobbyItem in lobbyEntries.Values) {
            Destroy(lobbyItem.gameObject);
        }
        lobbyEntries.Clear();
    }
}