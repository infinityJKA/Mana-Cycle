using Battle.Board;
using UnityEngine;
using UnityEngine.InputSystem;
using VersusMode;

public class SoloBoardController : MonoBehaviour {
    [SerializeField] private GameBoard board;

    [SerializeField] private InputActionReference pieceMoveAnalogAction;
    [SerializeField] private InputActionReference pieceTapLeftAction, pieceTapRightAction, quickfallButtonAction, castAction, abilityAction, pauseAction, rotateCCWAction, rotateCWAction;

    private Vector2 joystickInput;
    private static float joystickDeadzone = 0.1f;
    private static float joystickInputMagnitude = 0.5f;

    private bool joystickPressed;
    bool joystickPressedSouth = false;
    bool quickfallButtonPressed = false;

    private void Awake() {
        // Only use this object if there is no second player and no need for multiple device handling. (PlayerConnectionHandler will destroy itself if not)
        if (Storage.isPlayerControlled2) Destroy(gameObject);
    }

    private void Update() {
        joystickInput = pieceMoveAnalogAction.action.ReadValue<Vector2>();

        // navigation handling for new input system
        if (joystickPressed) {
            if (joystickInput.magnitude <= joystickDeadzone) {
                joystickPressed = false;
                board.quickFall = quickfallButtonPressed || joystickPressedSouth;
            }
        }

        else if (!joystickPressed && joystickInput.magnitude >= joystickInputMagnitude) {
            joystickPressed = true;

            float angle = Vector2.SignedAngle(Vector2.up, joystickInput);
            Debug.Log("angle");

            if (Mathf.Abs(angle) < 45f) board.UseAbility();
            else if (Mathf.Abs(angle - 180f) < 45f) {
                joystickPressedSouth = true;
                board.quickFall = quickfallButtonPressed || joystickPressedSouth;
            }
            else if (Mathf.Abs(angle - 90f) < 45f) board.MoveLeft();
            else if (Mathf.Abs(angle + 90f) < 45f) board.MoveRight();
        }
    }

    private void OnEnable() {
        quickfallButtonAction.action.performed += OnQuickfallPressed;
        quickfallButtonAction.action.canceled += OnQuickfallReleased;
        pieceTapLeftAction.action.performed += OnPieceTapLeft;
        pieceTapRightAction.action.performed += OnPieceTapRight;
        rotateCCWAction.action.performed += OnRotateCCW;
        rotateCWAction.action.performed += OnRotateCW;
        castAction.action.performed += OnSpellcast;
        abilityAction.action.performed += OnAbiltyUse;
        pauseAction.action.performed += OnPause;
    }

    private void OnDisable() {
        quickfallButtonAction.action.performed -= OnQuickfallPressed;
        quickfallButtonAction.action.canceled -= OnQuickfallReleased;
        pieceTapLeftAction.action.performed -= OnPieceTapLeft;
        pieceTapRightAction.action.performed -= OnPieceTapRight;
        rotateCCWAction.action.performed -= OnRotateCCW;
        rotateCWAction.action.performed -= OnRotateCW;
        castAction.action.performed -= OnSpellcast;
        abilityAction.action.performed -= OnAbiltyUse;
        pauseAction.action.performed -= OnPause;

    }

    private void OnQuickfallPressed(InputAction.CallbackContext ctx) {
        quickfallButtonPressed = true;
        board.quickFall = quickfallButtonPressed || joystickPressedSouth;
        board.instaDropThisFrame = true;
    }

    private void OnQuickfallReleased(InputAction.CallbackContext ctx) {
        quickfallButtonPressed = false;
        board.quickFall = quickfallButtonPressed || joystickPressedSouth;
    }

    private void OnPieceTapLeft(InputAction.CallbackContext ctx) {
        Debug.Log("piece tapped left");
        board.MoveLeft();
    }

    private void OnPieceTapRight(InputAction.CallbackContext ctx) {
        Debug.Log("piece tapped right");
        board.MoveRight();
    }

    private void OnRotateCCW(InputAction.CallbackContext ctx) {
        Debug.Log("piece tapped rotated CCW");
        board.RotateCCW();
    }

    private void OnRotateCW(InputAction.CallbackContext ctx) {
        Debug.Log("piece tapped rotated CW");
        board.RotateCW();
    }

    private void OnSpellcast(InputAction.CallbackContext ctx) {
        Debug.Log("spellcasted");
        board.Spellcast();
    }

    private void OnAbiltyUse(InputAction.CallbackContext ctx) {
        Debug.Log("ability used");
        board.UseAbility();
    }

    private void OnPause(InputAction.CallbackContext ctx) {
        Debug.Log("paused");
        board.Pause();
    }
}