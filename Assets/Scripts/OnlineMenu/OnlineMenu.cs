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
using UnityEngine.InputSystem;
using System;

public class OnlineMenu : MonoBehaviour {
    public static OnlineMenu singleton;
    public TMPro.TMP_InputField joinCodeField, networkAddressField;
    public Button hostButton, joinButton;

    public GameObject onlineMenu, charSelectMenu;

    public InputActionReference backAction;


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
        if (!CheckOnline()) return;

        DisableInteractables();
        if (networkAddressField) NetworkManager.singleton.networkAddress = networkAddressField.text;
        
        try {
            // STEAM
            if (NetManager.IsUseSteam() && SteamManager.Initialized) {
                SteamLobbyManager.CreateLobby();
            } 
            // RELAY
            else if (RelayManager.relayNetworkManager != null) {
                bool success = await RelayManager.CreateRelay();
                if (!success) EnableInteractables();
            } 
            // IP
            else {
                NetworkManager.singleton.StartHost();
            }
        } catch (Exception e) {
            PopupManager.instance.ShowError(e);
            EnableInteractables();
        }
    }

    public async void JoinButtonPressed() {
        string joinCode = joinCodeField.text;

        if (!CheckOnline()) return;

        DisableInteractables();
        if (networkAddressField) NetworkManager.singleton.networkAddress = networkAddressField.text;
        
        try {
            // ======== STEAM
            if (NetManager.IsUseSteam() && SteamManager.Initialized) {
                // TODO: implement join via friends list or soemthing similar
                SteamLobbyManager.JoinLobbyWithID(joinCode);
                // NetworkManager.singleton.StartClient();
            } 
            // ======== RELAY
            else if (RelayManager.relayNetworkManager != null) {
                // validate code length
                if (joinCode.Length != 6) {
                    PopupManager.instance.ShowErrorMessage("Enter a valid join code");
                    EnableInteractables();
                    return;
                }
                bool success = await RelayManager.JoinRelay(joinCode);
                if (!success) EnableInteractables();
            } 
            // ======== IP (local)
            else {
                NetworkManager.singleton.StartClient();
            }
        }

        catch (Exception e) {
            PopupManager.instance.ShowError(e);
            EnableInteractables();
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
        charSelectMenu.SetActive(false);
        onlineMenu.SetActive(true);
        EnableInteractables();
    }

    public static bool CheckOnline() {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            PopupManager.instance.ShowErrorMessage("You seem to be offline. Check your internet connection!");
            return false;
        }

        return true;
    }
}