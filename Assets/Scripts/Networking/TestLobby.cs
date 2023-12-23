using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour {

    private Lobby hostLobby;

    private Lobby joinedLobby;

    private float heartbeatTimer;

    private List<Lobby> lobbies;

    private string playerName = "TestPlayer" + UnityEngine.Random.Range(100, 999);

    private async void Start() {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in "+AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        CreateLobby();

        QueryLobbies();

        QuickJoinLobby();
    }

    IEnumerator LobbyHeartbeat() {
        var delay = new WaitForSeconds(15f);
        while (hostLobby != null) {
            yield return delay;

            LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    IEnumerator LobbyPollForUpdates() {
        var delay = new WaitForSeconds(1.5f);
        while (joinedLobby != null) {
            yield return delay;

            LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
        }
    }

    private async void CreateLobby() {
        string lobbyName = "TestLobby";
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
            IsPrivate = false,
            Player = GetPlayer(),
        };

        try {
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;

            StartCoroutine(LobbyHeartbeat());

            Debug.Log("Created lobby: "+lobby.Name+" id: "+lobby.Id+" code: "+lobby.LobbyCode);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void QueryLobbies() {
        QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
            Count = 25,
            Filters = new List<QueryFilter> {
                // only lobbies with greater than 0 avilable slots
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
            },
            Order = new List<QueryOrder> {
                // Sort from oldest to newest
                new QueryOrder(false, QueryOrder.FieldOptions.Created)
            }
        };

        try {
            var lobbyQuery = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            lobbies = lobbyQuery.Results;

            Debug.Log("Lobbied found: "+lobbies.Count);
            foreach (Lobby lobby in lobbies) {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby() {
        try {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            StartCoroutine(LobbyPollForUpdates());
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode) {
        try {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer()
            };
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            StartCoroutine(LobbyPollForUpdates());

            ListPlayers(joinedLobby);

            Debug.Log("Joined lobby with code "+joinedLobby.LobbyCode);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private Player GetPlayer() {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
            }
        };
    }

    private void ListPlayers(Lobby lobby) {
        Debug.Log("Players in lobby "+lobby.Name+": "+lobby.Players.Count);
        foreach (Player player in lobby.Players) {
            Debug.Log(player.Data["PlayerName"].Value);
        }
    }

    private void UpdatePlayerName(string newPlayerName) {
        playerName = newPlayerName;
        LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
            Data = new Dictionary<string, PlayerDataObject> {
                { "PlayerName",  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
            }
        });
    }

    private async void LeaveLobby() {
        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void KickPlayer() {
        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby() {
        try {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
}