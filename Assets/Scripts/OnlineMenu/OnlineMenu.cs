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

    public SwapPanelManager swapPanelManager;


    private void Awake() {
        singleton = this;
    }

    private void Start() {
        if (networkAddressField) networkAddressField.text = NetworkManager.singleton.networkAddress;
        if (!Storage.online) {
            ShowCharSelect();
            enabled = false;
        }
    }

    private void OnEnable() {
        backAction.action.performed += OnBack;
        backAction.action.Enable();
    }
    private void OnDisable() {
        backAction.action.performed -= OnBack;
    }

    public void OnBack(InputAction.CallbackContext ctx) {
        if (!Storage.online) return;
        if (SidebarUI.instance && SidebarUI.instance.expanded) return;

        if (swapPanelManager && swapPanelManager.currentPanel != 0) {
            swapPanelManager.OpenPanel(0);
        } else {
            Back();
        }
    }

    public void Back() {
        TransitionScript.instance.WipeToScene("MainMenu", reverse: true);
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
        if (joinCodeField) joinCodeField.interactable = true;
        if (hostButton) hostButton.interactable = true;
        if (joinButton) joinButton.interactable = true;
    } 

    public void DisableInteractables() {
        if (joinCodeField) joinCodeField.interactable = false;
        if (hostButton) hostButton.interactable = false;
        if (joinButton) joinButton.interactable = false;
    }

    public void ShowCharSelect() {
        if (onlineMenu) onlineMenu.SetActive(false);
        if (charSelectMenu) charSelectMenu.SetActive(true);
    }

    public void ShowOnlineMenu() {
        if (charSelectMenu) charSelectMenu.SetActive(false);
        if (onlineMenu) onlineMenu.SetActive(true);
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