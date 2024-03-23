using MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SidebarUI : MonoBehaviour {
    public static SidebarUI instance {get; private set;}

    [SerializeField] private Animator animator;
    public bool expanded {get; private set;} = false;

    // panels that may be shown/hidden based on login or some other state
    [SerializeField] private GameObject usernamePanel, walletPanel, levelPanel;

    // Shown while logged in
    [SerializeField] private TMP_Text usernameLabel, coinCountLabel, iridiumCountLabel,
    levelLabel, xpLabel;

    // notifiers that popup based on login/other states
    [SerializeField] private GameObject offlineNotifier, loggingInNotifier;

    [SerializeField] private InputActionReference toggleAction;

    
    // Note: this class will not be DontDestroyOnLoad()ed but current one in scene will be saved for ref by other scenes.
    private void Awake() {
        instance = this;
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
        ToggleExpanded();
        Menu3d.instance.SelectLastSelected();
    }

    public void ToggleExpanded() {
        expanded = !expanded;
        animator.SetBool("expanded", expanded);
    }

    public void UpdatePlayerInfo() {
        UpdateUserInfo();
        UpdateWalletDisplay();
        UpdateXPDisplay();
    }

    public void LoginGuestPressed() {
        PlayerManager.LoginGuest();
    }

    public void LoginSteamPressed() {
        PlayerManager.LoginSteam();
    }

    public void SetCoins(string amount) {
        coinCountLabel.text = amount;
    }

    public void SetIridium(string amount) {
        iridiumCountLabel.text = amount;
    }

    public void UpdateUserInfo() {
        loggingInNotifier.SetActive(PlayerManager.loginInProgress);
        // only show offline notifier if not in the process of logging in
        if (!PlayerManager.loginInProgress) offlineNotifier.SetActive(PlayerManager.loginMode == PlayerManager.LoginMode.Local);
        
        usernameLabel.text = PlayerManager.playerUsername;
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


    public void OnAccountPressed()
    {
        Debug.Log("Todo: show account info details");
    }
    // htp: Menu3d.SelectHTP

    public void OnCosmeticsPressed() {
        Debug.Log("Todo: Transition to cosmetic menu scene");
    }

    public void OnShopPressed()
    {
        TransitionScript.instance.WipeToScene("CosmeticShop");
    }

    public void OnNewsPressed()
    {
        Debug.Log("Todo: show announcements/changelon window");
    }

    // options: Menu3d.SelectSettings
    // exit: Menu3d.SelectExit
}