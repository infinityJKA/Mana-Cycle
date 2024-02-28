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

    public TMPro.TMP_InputField joinCodeField, networkAddressField;
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

    public async void JoinButtonPressed() {
        DisableInteractables();
        if (networkAddressField) NetworkManager.singleton.networkAddress = networkAddressField.text;
        if (NetworkManagerManager.networkManagerType == NetworkManagerManager.NetworkManagerType.Steam) {
            // TODO: implement join via friends list or soemthing similar
            NetworkManager.singleton.StartClient();
        } else if (NetworkManagerManager.networkManagerType == NetworkManagerManager.NetworkManagerType.Relay) {
            await RelayManager.JoinRelay(joinCodeField.text);
        }
    }

    public void EnableInteractables() {
        joinCodeField.interactable = true;
        hostButton.interactable = true;
        joinButton.interactable = true;
    } 

    public void DisableInteractables() {
        joinCodeField.interactable = false;
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