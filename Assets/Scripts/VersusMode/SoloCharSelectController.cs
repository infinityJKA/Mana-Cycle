using UnityEngine;
using UnityEngine.InputSystem;
using VersusMode;

public class SoloCharSelectController : MonoBehaviour {
    [SerializeField] private CharSelectMenu charSelectMenu;

    private CharSelector charSelector;

    [SerializeField] private InputActionReference navigateAction, selectAction, backAction, pauseAction, abilityInfoAction, settingsAction;

    private Vector2 navigateInput;
    private static float joystickDeadzone = 0.1f;
    private static float joystickInputMagnitude = 0.5f;

    private bool joystickPressed;

    private void Awake() {
        // Only use this object if there is no second player and no need for multiple device handling. (PlayerConnectionHandler will destroy itself if not)
        if (Storage.isPlayerControlled2) Destroy(gameObject);

        charSelector = charSelectMenu.GetActiveSelector();
    }

    private void Update() {
        navigateInput = navigateAction.action.ReadValue<Vector2>();

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

    private void OnEnable() {
        selectAction.action.performed += OnSelect;
        backAction.action.performed += OnBack;
        pauseAction.action.performed += OnPause;
        abilityInfoAction.action.performed += OnAbilityInfo;
        settingsAction.action.performed += OnSettings;
    }

    private void OnDisable() {
        selectAction.action.performed -= OnSelect;
        backAction.action.performed -= OnBack;
        pauseAction.action.performed -= OnPause;
        abilityInfoAction.action.performed -= OnAbilityInfo;
        settingsAction.action.performed -= OnSettings;
    }

    private void OnSelect(InputAction.CallbackContext ctx) {
        charSelector.OnCast();
        charSelector = charSelectMenu.GetActiveSelector();
    }

    private void OnBack(InputAction.CallbackContext ctx) {
        charSelector.OnBack();
        charSelector = charSelectMenu.GetActiveSelector();
    }

    private void OnPause(InputAction.CallbackContext ctx) {
        charSelector.ReturnToMenu();
    }

    private void OnAbilityInfo(InputAction.CallbackContext ctx) {
        charSelector.OnRotateCCW();
    }

    private void OnSettings(InputAction.CallbackContext ctx) {
        charSelector.OnRotateCW();
    }
}