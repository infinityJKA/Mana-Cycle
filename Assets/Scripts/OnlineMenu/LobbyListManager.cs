using System.Collections.Generic;
using Networking;
using Steamworks;
using UnityEngine;

public class LobbyListManager : MonoBehaviour {
    [SerializeField] private GameObject lobbyEntryPrefab;
    [SerializeField] private Transform lobbyListContentTransform;

    private List<GameObject> lobbyEntries;

    public static LobbyListManager instance;

    private void Awake() {
        instance = this;
        lobbyEntries = new List<GameObject>();

        foreach (Transform child in lobbyListContentTransform) {
            Destroy(child.gameObject);
        }
    }

    private void Start() {
        SteamLobbyManager.GetLobbiesList();
    }

    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t result) {
        for (int i = 0; i < lobbyIDs.Count; i++) {
            if (lobbyIDs[i].m_SteamID == result.m_ulSteamIDLobby) {
                GameObject createdItem = Instantiate(lobbyEntryPrefab);
                var lobbyEntry = createdItem.GetComponent<LobbyEntry>();
                
                CSteamID id = (CSteamID)lobbyIDs[i].m_SteamID;
                string name = SteamMatchmaking.GetLobbyData(id, "name");

                lobbyEntry.SetLobbyData(id, name);

                createdItem.transform.SetParent(lobbyListContentTransform, false);

                lobbyEntries.Add(createdItem);
            }
        }
    }

    public void DestroyLobbyEntries() {
        foreach (GameObject lobbyItem in lobbyEntries) {
            Destroy(lobbyItem.gameObject);
        }
        lobbyEntries.Clear();
    }
}