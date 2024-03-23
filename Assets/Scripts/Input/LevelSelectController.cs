using ConvoSystem;
using SoloMode;
using UnityEngine;
using UnityEngine.InputSystem;
using VersusMode;

public class LevelSelectController : MonoBehaviour {
    [SerializeField] private LevelLister levelLister;
    [SerializeField] private ConvoHandler convoHandler;

    // [SerializeField] private InputActionReference navigateAction, selectAction, backAction, pauseAction, abilityInfoAction, settingsAction;

    private Vector2 navigateInput;
    private static float joystickDeadzone = 0.1f;
    private static float joystickInputMagnitude = 0.5f;

    private bool joystickPressed;

    private bool hasControl => !PopupUI.showingPopup;
    
    public void OnNavigate(InputAction.CallbackContext ctx) {
        if (convoHandler && convoHandler.enabled) return;
        if (!hasControl) return;
        if (SidebarUI.instance && SidebarUI.instance.expanded) return;

        navigateInput = ctx.ReadValue<Vector2>();

        // navigation handling for new input system
        if (joystickPressed) {
            if (navigateInput.magnitude <= joystickDeadzone) joystickPressed = false;
        }

        else if (!joystickPressed && navigateInput.magnitude >= joystickInputMagnitude) {
            joystickPressed = true;

            float angle = Vector2.SignedAngle(Vector2.up, navigateInput);

            if (Mathf.Abs(angle) < 45f) levelLister.MoveCursor(-1);
            else if (Mathf.Abs(angle - 180f) < 45f) levelLister.MoveCursor(1);
            else if (Mathf.Abs(angle - 90f) < 45f) levelLister.LeftTabArrow();
            else if (Mathf.Abs(angle + 90f) < 45f) levelLister.RightTabArrow();
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
        if (!ctx.performed) return;
        if (!hasControl) return;
        if (SidebarUI.instance && SidebarUI.instance.expanded) return;
        if (convoHandler.enabled) {
            if (!Storage.levelSelectedThisInput) convoHandler.Advance();
        } else {
            levelLister.ConfirmSelectedLevel();
        }
    }

    public void OnInfo(InputAction.CallbackContext ctx) {
        if (!ctx.performed) return;
        if (!hasControl) return;
        if (convoHandler.enabled) return;
        if (SidebarUI.instance && SidebarUI.instance.expanded) return;

        levelLister.ToggleInfo();
    }

    public void OnBack(InputAction.CallbackContext ctx) {
        if (!ctx.performed) return;
        if (!hasControl) return;
        if (SidebarUI.instance && SidebarUI.instance.expanded) return;
        if (convoHandler.enabled) {
            convoHandler.EndConvo();
        } else {
            levelLister.Back();
        }
    }
}