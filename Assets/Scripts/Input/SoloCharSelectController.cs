using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using VersusMode;

public class SoloCharSelectController : MonoBehaviour {
    public static SoloCharSelectController instance;

    [SerializeField] private CharSelectMenu charSelectMenu;

    /// <summary>
    /// In online mode, the (client) net player the client is controlling
    /// </summary>
    public NetPlayer netPlayer;

    private CharSelector charSelector;

    // [SerializeField] private InputActionReference navigateAction, selectAction, backAction, pauseAction, abilityInfoAction, settingsAction;

    private Vector2 navigateInput;
    private static float joystickDeadzone = 0.1f;
    private static float joystickInputMagnitude = 0.5f;

    private bool joystickPressed;

    bool canControl => charSelectMenu && charSelectMenu.gameObject.activeInHierarchy && !PopupManager.showingPopup;

    private void Awake() {
        instance = this;

        // Only use this object if there is no second player and no need for multiple device handling. (PlayerConnectionHandler will destroy itself if not)
        // also use if in online mode, where other player will be controlled by the other net client
        if (Storage.gamemode != Storage.GameMode.Solo && Storage.isPlayerControlled2 && !Storage.online) Destroy(gameObject);

        charSelector = charSelectMenu.GetActiveSelector();
    }
    

    public void OnNavigate(InputAction.CallbackContext ctx) {
        if (!canControl) return;

        if (SidebarUI.instance && SidebarUI.instance.expanded) return;

        navigateInput = ctx.action.ReadValue<Vector2>();

        // navigation handling for new input system
        if (joystickPressed) {
            if (navigateInput.magnitude <= joystickDeadzone) joystickPressed = false;
        }

        else if (!joystickPressed && navigateInput.magnitude >= joystickInputMagnitude) {
            joystickPressed = true;

            float angle = Vector2.SignedAngle(Vector2.up, navigateInput);

            if (Mathf.Abs(angle) < 45f) charSelector.OnMoveUp();
            else if (Mathf.Abs(angle - 180f) < 45f) charSelector.OnMoveDown();
            else if (Mathf.Abs(angle - 90f) < 45f) charSelector.OnMoveLeft();
            else if (Mathf.Abs(angle + 90f) < 45f) charSelector.OnMoveRight();

            if (Storage.online && NetworkClient.active) {
                netPlayer.CmdSetSelectedBattlerIndex(charSelector.selectedIcon.index);
                // if (netPlayer.isClient) {
                //     netPlayer.CmdSetSelectedBattlerIndex(charSelector.selectedIcon.index);
                // } else {
                //     netPlayer.RpcSetSelectedBattlerIndex(charSelector.selectedIcon.index);
                // }
            }
        }
    }

    // public void OnEnable() {
    //     selectAction.action.performed += OnSelect;
    //     backAction.action.performed += OnBack;
    //     pauseAction.action.performed += OnPause;
    //     abilityInfoAction.action.performed += OnAbilityInfo;
    //     settingsAction.action.performed += OnSettings;
    // }

    // public void OnDisable() {
    //     selectAction.action.performed -= OnSelect;
    //     backAction.action.performed -= OnBack;
    //     pauseAction.action.performed -= OnPause;
    //     abilityInfoAction.action.performed -= OnAbilityInfo;
    //     settingsAction.action.performed -= OnSettings;
    // }

    public void OnSelect(InputAction.CallbackContext ctx) {
        if (!canControl) return;

        if (SidebarUI.instance && SidebarUI.instance.expanded) return;

        if (!ctx.performed) return;
        charSelector.OnCast(true);

        if (Storage.online && NetworkClient.active) {
            if (charSelector.menu.started) {
                netPlayer.CmdStartGame();
            } else {
                netPlayer.CmdSetLockedIn(charSelector.selectedIcon.index, charSelector.isRandomSelected, charSelector.lockedIn);
            }
        } else {
            charSelector = charSelectMenu.GetActiveSelector();
        }
    }

    public void OnBack(InputAction.CallbackContext ctx) {
        

        // dont handle back or pause while shift pressed (messes up steam menu)
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) return;

        if (ctx.canceled) {
            charSelector.ReturnMenuUnpress();
            charSelector = charSelectMenu.GetActiveSelector();
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;
        }

        if (!ctx.performed) return;
        OnPauseOrBack();
    }

    // public void OnPause(InputAction.CallbackContext ctx) {
    //     // dont handle back or pause while shift pressed (messes up steam menu)
    //     if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) return;

    //     if (ctx.canceled) {
    //         charSelector.ReturnMenuUnpress();
    //         charSelector = charSelectMenu.GetActiveSelector();
    //     }

    //     if (!ctx.performed) return;
    //     if (SidebarUI.instance && SidebarUI.instance.expanded) return;
    //     OnPauseOrBack();
    //     // charSelector.ReturnMenuPress();
    // }

    private void OnPauseOrBack() {
        if (charSelectMenu.gameObject.activeInHierarchy) {
            charSelector.OnBack();
            charSelector = charSelectMenu.GetActiveSelector();
            if (charSelector.lockedIn) charSelector.ToggleLock();
            if (netPlayer && NetworkClient.active) netPlayer.CmdSetLockedIn(charSelector.selectedIcon.index, charSelector.isRandomSelected, charSelector.lockedIn);
        } else { // online menu
            TransitionScript.instance.WipeToScene("MainMenu", reverse: true);
        }
    }

    public void OnAbilityInfo(InputAction.CallbackContext ctx) {
        if (SidebarUI.instance && SidebarUI.instance.expanded) return;

        if (!ctx.performed) return;
        if (!canControl) return; 
        charSelector.OnAbilityInfo();
    }

    public void OnSettings(InputAction.CallbackContext ctx) {
        if (SidebarUI.instance && SidebarUI.instance.expanded) return;

        if (!canControl) return;
        if (!ctx.performed) return;
        charSelector.OnSettings();
    }
}