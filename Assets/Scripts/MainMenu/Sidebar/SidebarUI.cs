using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SidebarUI : MonoBehaviour {
    public static SidebarUI instance {get; private set;}

    [SerializeField] private Animator animator;
    public bool expanded {get; private set;} = false;

    // windows
    [SerializeField] private GameObject playerInfoWindow, loginOptionsWindow;

    // Shown while logged in
    [SerializeField] private TMP_Text usernameLabel, coinCountLabel, iridiumCountLabel,
    levelLabel, xpLabel;

    [SerializeField] private InputActionReference toggleAction;

    
    // Note: this class will not be DontDestroyOnLoad()ed but current one in scene will be saved for ref by other scenes.
    // set instance on start & show appropriate data.
    private void Awake() {
        instance = this;

        if (PlayerManager.loggedIn) {
            ShowPlayerInfo();
        } else {
            ShowLoginOptions();
        }
    }

    private void OnEnable() {
        toggleAction.action.performed += OnTogglePressed;
        toggleAction.action.Enable(); // i dont know how but pause got disabled, probably something to do with rebinds
    }

    private void OnDisable() {
        toggleAction.action.performed -= OnTogglePressed;
    }

    private void OnTogglePressed(InputAction.CallbackContext ctx) {
        ToggleExpanded();
    }

    public void ToggleExpanded() {
        expanded = !expanded;
        animator.SetBool("expanded", expanded);
    }

    public void ShowLoginOptions() {
        playerInfoWindow.SetActive(false);
        loginOptionsWindow.SetActive(true);
    }

    public void ShowPlayerInfo() {
        loginOptionsWindow.SetActive(false);
        playerInfoWindow.SetActive(true);

        usernameLabel.text = PlayerManager.playerUsername;
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

    public void UpdateWalletDisplay() {
        coinCountLabel.text = WalletManager.coins;
        iridiumCountLabel.text = WalletManager.iridium;
    }

    public void UpdateXPDisplay() {
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