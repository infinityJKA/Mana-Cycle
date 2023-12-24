using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineJoinMenu : MonoBehaviour {
    [SerializeField] private TMP_Text usernameInputField;
    [SerializeField] private LobbyManager lobbyManager;

    private string usernameEntered;

    public async Task UpdateUsername() {
        await lobbyManager.UpdatePlayerName(usernameEntered);
    }

    public async void QuickPlayPressed() {
        await Authenticate();
        bool lobbyJoined = await lobbyManager.QuickJoinLobby();
        if (!lobbyJoined) {
            await lobbyManager.CreateLobby();
        }
    }

    public async void CreateLobbyPressed() {
        await Authenticate();
        await lobbyManager.CreateLobby();
    }

    public async Task Authenticate() {
        // note: later on when authentication is actually important, this may be done through steam/google/builtin in crappy database idk
        await lobbyManager.Authenticate(usernameInputField.text);
    }

    public void JoinByCodePressed() {
        // TODO: make a seperate window to enter code & join.
    }

    public void OnUsernameFieldChanged(string newUsername) {
        usernameEntered = newUsername;
    }
}