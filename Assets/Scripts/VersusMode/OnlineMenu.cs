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
        DisableInteractables();
        NetworkManager.singleton.StartHost();
    }

    public void JoinButtonPressed() {
        DisableInteractables();
        NetworkManager.singleton.StartClient();
    }

    public void EnableInteractables() {
        inputField.interactable = true;
        hostButton.interactable = true;
        joinButton.interactable = true;
    } 

    public void DisableInteractables() {
        inputField.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
    }

    public void ShowCharSelect() {
        onlineMenu.SetActive(false);
        charSelectMenu.SetActive(true);
    }

    public void ShowOnlineMenu() {
        Debug.Log("showing online menu");
        charSelectMenu.SetActive(false);
        onlineMenu.SetActive(true);
    }
}