using Cosmetics;
using MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SidebarUI : MonoBehaviour {
    public static SidebarUI instance {get; private set;}

    [SerializeField] private Animator animator;
    public bool expanded {get; private set;} = false;

    // Top window based on logged in state
    [SerializeField] private GameObject playerInfoWindow, loggedOutWindow;
    [SerializeField] private Button mainButtonFirstSelected, loginOptionsFirstSelected;

    // Bottom buttons window that may be swapped based on login process / other things
    [SerializeField] private GameObject mainButtonsWindow, loginOptionsWindow;
    private bool showingLoginOptions = false;
    private GameObject loginOptionsLastSelected = null;

    // error shown on login options window if login failed
    [SerializeField] private TMP_Text loginErrorLabel;

    // sub-panels that may be shown/hidden based on login or some other state
    [SerializeField] private GameObject usernamePanel, walletPanel, levelPanel;

    // Shown while logged in
    [SerializeField] private TMP_Text usernameLabel, coinCountLabel, iridiumCountLabel,
    levelLabel, xpLabel;

    // text to change based on login / other state
    [SerializeField] private TMP_Text accountButtonLabel;

    // notifiers that popup based on login/other states
    [SerializeField] private GameObject offlineNotifier, loggingInNotifier;

    [SerializeField] private InputActionReference toggleAction;

    [SerializeField] private bool selectStoredOnBack = true;

    // Note: this class will not be DontDestroyOnLoad()ed but current one in scene will be saved for ref by other scenes.
    private void Awake() {
        instance = this;
        Storage.lastSidebarItem = mainButtonFirstSelected.gameObject;
    }

    private void Start() {
        UpdatePlayerInfo();
    }

    private void OnEnable() {
        toggleAction.action.performed += OnTogglePressed;
        toggleAction.action.Enable(); // i dont know how but pause action got disabled, probably something to do with rebinds
        animator.SetBool("expanded", expanded);
    }

    private void OnDisable() {
        toggleAction.action.performed -= OnTogglePressed;
    }

    private void OnTogglePressed(InputAction.CallbackContext ctx) {
        if (PopupManager.showingPopup) return;

        ToggleExpanded();

        if (expanded) {
            // todo: use a global lastSelected, possibly in storage, for compatabilitiy with all scenes
            Storage.storedSelection = EventSystem.current.currentSelectedGameObject;
            SelectLastSelected();
        } else {
            if (selectStoredOnBack) {
                ReselectAfterClose();
            } else {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public static void ReselectAfterClose() {
        if (Menu3d.instance) {
            Menu3d.instance.SelectLastSelected();
        } else if (Storage.storedSelection) {
            EventSystem.current.SetSelectedGameObject(Storage.storedSelection);
        } else {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ToggleExpanded() {
        expanded = !expanded;

        if (showingLoginOptions) {
            showingLoginOptions = false;
            UpdateButtonsWindow();
        }

        PlayerManager.loginError = "";
        UpdateUserInfo();

        animator.SetBool("expanded", expanded);
    }

    public void UpdatePlayerInfo() {
        // only show logged out window if logged out and not currently a login in progress.
        loggedOutWindow.SetActive(!PlayerManager.loggedIn);
        playerInfoWindow.SetActive(PlayerManager.loggedIn);

        if (!PlayerManager.loggedIn && !PlayerManager.loginInProgress) {
            accountButtonLabel.text = "Login";
        } else if (PlayerManager.loggedIn) {
            accountButtonLabel.text = "Account";
        }

        UpdateUserInfo();
        UpdateWalletDisplay();
        UpdateXPDisplay();
    }

    public void SetCoins(string amount) {
        coinCountLabel.text = amount;
    }

    public void SetIridium(string amount) {
        iridiumCountLabel.text = amount;
    }

    public void UpdateButtonsWindow() {
        // hide showing interactables menu if logged in
        if (showingLoginOptions) {
            if (PlayerManager.loggedIn) {
                showingLoginOptions = false;
            } else {
                SetLoginButtonsInteractable(!PlayerManager.loginInProgress);
                if (loginOptionsLastSelected) {
                    EventSystem.current.SetSelectedGameObject(loginOptionsLastSelected);
                    loginOptionsLastSelected = null;
                }
            }
        }

        loginOptionsWindow.SetActive(showingLoginOptions);
        mainButtonsWindow.SetActive(!showingLoginOptions);
    }

    public void UpdateUserInfo() {
        loggingInNotifier.SetActive(PlayerManager.loginInProgress);

        // only show offline notifier if not in the process of logging in and also logged out (not online)
        offlineNotifier.SetActive(!PlayerManager.loginInProgress && Application.internetReachability == NetworkReachability.NotReachable);

        loginErrorLabel.text = PlayerManager.loginError;

        if (PlayerManager.loggedIn) {
            usernameLabel.text = PlayerManager.playerUsername;
        } else if (PlayerManager.loginInProgress) {
            usernameLabel.text = "Logging in...";
        }

    }

    public void UpdateWalletDisplay() {
        walletPanel.SetActive(usernamePanel);
        if (!PlayerManager.loggedIn) return;

        coinCountLabel.text = ""+WalletManager.coins;
        iridiumCountLabel.text = ""+WalletManager.iridium;
    }

    public void UpdateXPDisplay() {
        levelPanel.SetActive(usernamePanel);
        if (!PlayerManager.loggedIn) return;

        levelLabel.text = "Lv "+XPManager.level;
        xpLabel.text = XPManager.xp+"/"+XPManager.xpToNext;
    }

    // ==== main buttons
    public void OnAccountPressed()
    {
        // eventually this will probably have its own account window. but for now, just take to change username.
        // also, no logout functionality to player. as theres basically no point in logging out
        ChangeUsernamePressed();        
    }
    [SerializeField] private UsernamePopup usernamePopupPrefab;
    public void ChangeUsernamePressed() {
        if (PlayerManager.canChangeUsername) PopupManager.instance.ShowPopupFromPrefab(usernamePopupPrefab);
    }

    public void LogoutPressed() {
        if (PlayerManager.loginInProgress) return;
        if (PlayerManager.loggedIn) {
            // Log out when account button pressed (THIS IS TEMPORARY, account window will be added later where logout can be found)
            PlayerManager.Logout();
        } else {
            showingLoginOptions = true;
            PlayerManager.loginError = "";
            SetLoginButtonsInteractable(true);
            UpdateButtonsWindow();
            loginOptionsFirstSelected.Select();
        }
    }

    // htp: Menu3d.SelectHTP()

    public void OnCosmeticsPressed() {
        Debug.Log("Todo: Transition to cosmetic menu scene");
    }

    public void OnShopPressed()
    {
        CosmeticShop.sceneOnBack = SceneManager.GetActiveScene().name;
        TransitionScript.instance.WipeToScene("CosmeticShop");
    }

    public void OnNewsPressed()
    {
        Debug.Log("Todo: show announcements/changelon window");
    }

    // options: Menu3d.SelectSettings()
    // home: (depends on scene)
    // exit: Menu3d.SelectExit()
    

    // ==== login options buttons
    public void LoginBackPressed() {
        showingLoginOptions = false;
        UpdateButtonsWindow();
        SelectLastSelected();
    }

    public void LoginGuestPressed() {
        PlayerManager.loginError = "";
        PlayerManager.LoginGuest();
        LoginPressed();
    }

    public void LoginSteamPressed() {
        PlayerManager.loginError = "";
        PlayerManager.LoginSteam();
        LoginPressed();
    }

    public void LoginPressed() {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            PlayerManager.loginError = "You are offline";
        }

        loginOptionsLastSelected = EventSystem.current.currentSelectedGameObject;
        SetLoginButtonsInteractable(false);
        
        UpdateUserInfo();
        UpdateButtonsWindow();
        // todo: show a spinner or somethin to show that login is in progress
        // Once login process finishes, PlayerManager will call UpdateButtonsList() and UpdatePlayerInfo() on this instance
        // whish will show the appropriate data
    }

    public void SetLoginButtonsInteractable(bool interactable) {
        foreach (Button button in loginOptionsWindow.transform.GetComponentsInChildren<Button>()) {
            button.interactable = interactable;
        }
    }

    public void SelectLastSelected() {
        EventSystem.current.SetSelectedGameObject(Storage.lastSidebarItem);
    }
}