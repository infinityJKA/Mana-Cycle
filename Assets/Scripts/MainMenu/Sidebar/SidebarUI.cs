using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SidebarUI : MonoBehaviour {
    public static SidebarUI instance {get; private set;}

    [SerializeField] private Animator animator;
    private bool expanded = false;

    // windows
    [SerializeField] private GameObject playerInfoWindow, loginOptionsWindow;

    // Shown while logged in
    [SerializeField] private TMP_Text usernameLabel, coinCountLabel, iridiumCountLabel;

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
        Debug.Log("listening for "+toggleAction);
    }

    private void OnDisable() {
        toggleAction.action.performed -= OnTogglePressed;
        Debug.Log("stopped listening for "+toggleAction);
    }

    private void OnTogglePressed(InputAction.CallbackContext ctx) {
        ToggleExpanded();
    }

    public void ToggleExpanded() {
        expanded = !expanded;
        Debug.Log("expanded: "+expanded);
        animator.SetBool("expanded", expanded);
    }

    public void ShowLoginOptions() {
        playerInfoWindow.SetActive(false);
        loginOptionsWindow.SetActive(true);
    }

    public void ShowPlayerInfo() {
        loginOptionsWindow.SetActive(false);
        playerInfoWindow.SetActive(true);
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
}