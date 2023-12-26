using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainMenu {

    /// <summary>
    /// Controls the main menu buttons, the cinemachine camera targets in the 3dmenu & opens menus in the main menu.
    /// </summary>
    public class KonamiTracker : MonoBehaviour
    {
        public static KonamiTracker instance {get; private set;}


        [SerializeField] private BlackjackMenu blackjackMenu;

        string[] konamiCode = {"up", "up", "down", "down", "left", "right", "left", "right", "b", "a"};
        int konamiIndex = 0;

        private Vector2 navigateInput;
        private static float joystickDeadzone = 0.1f;
        private static float joystickInputMagnitude = 0.5f;

        private bool joystickPressed;

        [SerializeField] private InputActionReference navigate_action, b_action, a_action;

        private void Awake() {
            instance = this;
        }

        private void OnEnable() {
            b_action.action.Enable();
            b_action.action.performed += OnB;
            a_action.action.Enable();
            a_action.action.performed += OnA;
        }

        private void OnDisable() {
            b_action.action.performed -= OnB;
            a_action.action.performed -= OnA;
        }

        private void OnB(InputAction.CallbackContext ctx) {
            RegisterKonami("b");
        }

        private void OnA(InputAction.CallbackContext ctx) {
            RegisterKonami("a");
        }

        private void Update() {
            navigateInput = navigate_action.action.ReadValue<Vector2>();

            // navigation handling for new input system
            if (joystickPressed) {
                if (navigateInput.magnitude <= joystickDeadzone) joystickPressed = false;
            }

            else if (!joystickPressed && navigateInput.magnitude >= joystickInputMagnitude) {
                joystickPressed = true;

                float angle = Vector2.SignedAngle(Vector2.up, navigateInput);

                if (Mathf.Abs(angle) < 45f) RegisterKonami("up");
                else if (Mathf.Abs(angle - 180f) < 45f) RegisterKonami("down");
                else if (Mathf.Abs(angle - 90f) < 45f) RegisterKonami("left");
                else if (Mathf.Abs(angle + 90f) < 45f) RegisterKonami("right");
            }
        }

        private void RegisterKonami(string input)
        {
            if (konamiCode[konamiIndex] == input)
            {
                konamiIndex++;
                if (konamiIndex >= konamiCode.Length)
                {
                    konamiIndex = 0;
                    blackjackMenu.OpenBlackjack();
                }
            }
            else
            {
                if (konamiCode[0] == input) {
                    konamiIndex = 1;
                } else {
                    konamiIndex = 0;
                }
            }
            Debug.Log("konami index " + konamiIndex);
        }
    }
}
