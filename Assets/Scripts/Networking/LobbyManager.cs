using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour {

    private Lobby hostLobby;

    public Lobby joinedLobby {get; private set;}

    private List<Lobby> lobbies;

    [SerializeField] private TMP_Text debugPlayerLister;

    private float lobbyPollTime = 1.5f;


    private float lobbyPollTimer = 0f;

    private string playerName = "";

    // private async void Start() {
    //     await Authenticate();

    //     await QueryLobbies();

    //     if (lobbies.Count > 0) {
    //         await QuickJoinLobby();
    //     } else {
    //         await CreateLobby();
    //     }
    // }

    private void Update() {
        HandleLobbyPolling();
        DebugListPlayers();
    }

    private async void HandleLobbyPolling() {
        if (joinedLobby == null) return;
        lobbyPollTimer -= Time.deltaTime;
        if (lobbyPollTimer <= 0) {
            lobbyPollTimer = lobbyPollTime;
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;
        }
    }

    public async Task Authenticate(string name) {
        playerName = name.Trim();
        Debug.Log("Authenticating with username "+playerName);

        Regex rgx = new Regex("[^a-zA-Z0-9 -_]");
        playerName = rgx.Replace(playerName, "");

        if (playerName.Length == 0) {
            Debug.Log("Please enter a username");
            return;
        }

        if (playerName.Length > 16) {
            Debug.Log("Username is too long");
            return;
        }

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in: "+AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    IEnumerator LobbyHeartbeat() {
        var delay = new WaitForSeconds(15f);
        while (hostLobby != null) {
            yield return delay;

            LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    public async Task CreateLobby() {
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
            Debug.LogError(e);
        }
    }

    public async Task QueryLobbies() {
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
            Debug.LogError(e);
        }
    }

    public async Task<bool> QuickJoinLobby() {
        try {
            QuickJoinLobbyOptions joinLobbyByCodeOptions = new QuickJoinLobbyOptions {
                Player = GetPlayer()
            };
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinLobbyByCodeOptions);
            lobbyPollTimer = lobbyPollTime;
            Debug.Log("Quick-joined lobby with code "+joinedLobby.LobbyCode);
            return true;
        } catch (LobbyServiceException e) {
            Debug.LogError(e);
            return false;
        }
    }

    public async void JoinLobbyByCode(string lobbyCode) {
        try {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer()
            };
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            lobbyPollTimer = lobbyPollTime;

            Debug.Log("Joined lobby with code "+joinedLobby.LobbyCode);
        } catch (LobbyServiceException e) {
            Debug.LogError(e);
        }
    }

    public Player GetPlayer() {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
            }
        };
    }

    public void DebugListPlayers() {
        if (joinedLobby != null) {
            string text = "Players in lobby "+joinedLobby.Name+": "+joinedLobby.Players.Count+"\n";
            foreach (Player player in joinedLobby.Players) {
                text += player.Data["PlayerName"].Value+"\n";
            }
            debugPlayerLister.text = text;
        } else {
            debugPlayerLister.text = "No lobby joined";
        }
    }

    public async Task UpdatePlayerName(string newPlayerName) {
        if (newPlayerName == playerName) return;
        playerName = newPlayerName;
        await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
            Data = new Dictionary<string, PlayerDataObject> {
                { "PlayerName",  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
            }
        });
    }

    public async void LeaveLobby() {
        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        } catch (LobbyServiceException e) {
            Debug.LogError(e);
        }
    }

    public async void KickPlayer() {
        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        } catch (LobbyServiceException e) {
            Debug.LogError(e);
        }
    }

    public async void DeleteLobby() {
        try {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        } catch (LobbyServiceException e) {
            Debug.LogError(e);
        }
    }
}