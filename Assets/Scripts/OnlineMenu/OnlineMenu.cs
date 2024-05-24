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
using UnityEngine.EventSystems;
using TMPro;

public class OnlineMenu : MonoBehaviour {
    public static OnlineMenu singleton;
    // public TMP_InputField joinCodeField, networkAddressField;
    [SerializeField] private TMP_Text statusLabel;
    public Button hostButton, joinButton;
    public Toggle publicToggle;

    public GameObject onlineMenu, charSelectMenu;

    public InputActionReference backAction;

    public SwapPanelManager swapPanelManager;

    [SerializeField] private GameObject loadingIcon;


    private void Awake() {
        singleton = this;
    }

    private void Start() {
        // if (networkAddressField) networkAddressField.text = NetworkManager.singleton.networkAddress;
        if (!Storage.online) {
            ShowCharSelect();
            enabled = false;
        } else {
            hostButton.Select();
            statusLabel.text = "";
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
        } 
        // else {
        //     Back();
        // }
    }

    public void Back() {
        TransitionScript.instance.WipeToScene("MainMenu", reverse: true);
    }

    public void HostButtonPressed()
    {
        if (!CheckOnline()) return;

        // if (networkAddressField) NetworkManager.singleton.networkAddress = networkAddressField.text;

        try
        {
            // STEAM
            if (NetManager.IsUseSteam())
            {
                if (!SteamManager.Initialized) {
                    PopupManager.instance.ShowErrorMessage("Could not connect to Steam");
                    return;
                }
                statusLabel.text = "Creating lobby...";
                DisableInteractables();
                SteamLobbyManager.CreateLobby();
            }
            // RELAY
            else if (RelayManager.relayNetworkManager != null)
            {
                // bool success = await RelayManager.CreateRelay();
                // if (!success) EnableInteractables();
                PopupManager.instance.ShowBasicPopup("Message", "Online hosting not implemented outside of Steam");
            }
            // IP
            else
            {
                // NetworkManager.singleton.StartHost();
                PopupManager.instance.ShowBasicPopup("Message", "Online joining not implemented outside of Steam");
            }
        }
        catch (Exception e)
        {
            PopupManager.instance.ShowError(e);
            EnableInteractables();
        }
    }

    public async void JoinButtonPressed() {
        if (NetManager.IsUseSteam()) {
            #if !DISABLESTEAMWORKS
                if (!SteamManager.Initialized) {
                    PopupManager.instance.ShowErrorMessage("Could not connect to Steam");
                    return;
                }
                // PopupManager.instance.ShowBasicPopup("Message", "Press Shift+Tab to open the friend's list, and joina  player from there.\nOnline random matchmaking coming in the future.");
                Debug.Log("Opening friends list");
                SteamLobbyManager.OpenFriendsList();
            #else
                PopupManager.instance.ShowBasicPopup("Message", "Online joining not implemented outside of Steam");
            #endif
            return;
        }

        if (!CheckOnline()) return;

        DisableInteractables();
        // if (networkAddressField) NetworkManager.singleton.networkAddress = networkAddressField.text;


        
        try {
            // ======== STEAM
            if (NetManager.IsUseSteam() && SteamManager.Initialized) {
                // just show friends list so player can join friend from there. baiscally same as presing shift-tab
                // in future, online random matchmaking will probably be this button.
                SteamFriends.ActivateGameOverlay("friends");
            } 
            // ======== RELAY
            else if (RelayManager.relayNetworkManager != null) {
                // validate code length
                string joinCode = ""; // todo: once relay is added for online (if ever), use this
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
        // if (joinCodeField) joinCodeField.interactable = true;
        if (hostButton) hostButton.interactable = true;
        if (joinButton) joinButton.interactable = true;
        loadingIcon.SetActive(false);
    } 

    public void DisableInteractables() {
        // if (joinCodeField) joinCodeField.interactable = false;
        if (hostButton) hostButton.interactable = false;
        if (joinButton) joinButton.interactable = false;
        loadingIcon.SetActive(true);
    }

    public void ShowCharSelect() {
        if (onlineMenu) onlineMenu.SetActive(false);
        if (charSelectMenu) charSelectMenu.SetActive(true);
    }

    public void ShowOnlineMenu() {
        if (charSelectMenu) charSelectMenu.SetActive(false);
        if (onlineMenu) onlineMenu.SetActive(true);
        EnableInteractables();
        hostButton.Select();
        statusLabel.text = "";
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