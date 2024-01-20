using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class OnlineMenu : MonoBehaviour {
    public static OnlineMenu singleton;

    public TMPro.TMP_InputField inputField;
    public Button hostButton, joinButton;

    public GameObject onlineMenu, charSelectMenu;

    private void Awake() {
        singleton = this;
    }

    private void Start() {
        if (!Storage.online) {
            ShowCharSelect();
        }
    }

    public void HostButtonPressed() {
        inputField.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
        NetworkManager.singleton.StartHost();
    }

    public void JoinButtonPressed() {
        inputField.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
        NetworkManager.singleton.StartClient();
    }

    public void ShowCharSelect() {
        onlineMenu.SetActive(false);
        charSelectMenu.SetActive(true);
    }

    public void ShowOnlineMenu() {
        charSelectMenu.SetActive(false);
        onlineMenu.SetActive(true);
    }
}