#if !UNITY_STANDALONE
  #define DISABLESTEAMWORKS 
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

using Mirror;
using UnityEngine;
using UnityEngine.UI;

using Networking;

public class OnlineMenu : MonoBehaviour {
    public static OnlineMenu singleton;

    public TMPro.TMP_InputField inputField, networkAddressField;
    public Button hostButton, joinButton;

    public GameObject onlineMenu, charSelectMenu;


    private void Awake() {
        singleton = this;
    }

    private void Start() {
        if (networkAddressField) networkAddressField.text = NetworkManager.singleton.networkAddress;
        if (!Storage.online) {
            ShowCharSelect();
        }
    }

    public async void HostButtonPressed() {
        DisableInteractables();
        if (networkAddressField) NetworkManager.singleton.networkAddress = networkAddressField.text;
        if (NetworkManagerManager.networkManagerType == NetworkManagerManager.NetworkManagerType.Steam) {
            SteamLobbyManager.CreateLobby();
            NetworkManager.singleton.StartHost();
        } else if (NetworkManagerManager.networkManagerType == NetworkManagerManager.NetworkManagerType.Relay) {
            await RelayManager.CreateRelay();
        }
    }

    public void JoinButtonPressed() {
        DisableInteractables();
        if (networkAddressField) NetworkManager.singleton.networkAddress = networkAddressField.text;
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
        EnableInteractables();
    }
}