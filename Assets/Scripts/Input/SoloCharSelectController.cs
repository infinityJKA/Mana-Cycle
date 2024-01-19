using UnityEngine;
using UnityEngine.InputSystem;
using VersusMode;

public class SoloCharSelectController : MonoBehaviour {
    [SerializeField] private CharSelectMenu charSelectMenu;

    private CharSelector charSelector;

    // [SerializeField] private InputActionReference navigateAction, selectAction, backAction, pauseAction, abilityInfoAction, settingsAction;

    private Vector2 navigateInput;
    private static float joystickDeadzone = 0.1f;
    private static float joystickInputMagnitude = 0.5f;

    private bool joystickPressed;

    private void Awake() {
        // Only use this object if there is no second player and no need for multiple device handling. (PlayerConnectionHandler will destroy itself if not)
        if (Storage.isPlayerControlled2 || Storage.online) Destroy(gameObject);

        charSelector = charSelectMenu.GetActiveSelector();
    }

    public void OnNavigate(InputAction.CallbackContext ctx) {
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
        charSelector.OnCast(true);
        charSelector = charSelectMenu.GetActiveSelector();
    }

    public void OnBack(InputAction.CallbackContext ctx) {
        if (!ctx.performed) return;
        charSelector.OnBack();
        charSelector = charSelectMenu.GetActiveSelector();
    }

    public void OnPause(InputAction.CallbackContext ctx) {
        if (!ctx.performed) return;
        charSelector.ReturnToMenu();
    }

    public void OnAbilityInfo(InputAction.CallbackContext ctx) {
        if (!ctx.performed) return;
        charSelector.OnAbilityInfo();
    }

    public void OnSettings(InputAction.CallbackContext ctx) {
        if (!ctx.performed) return;
        charSelector.OnSettings();
    }
}