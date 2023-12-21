using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MainMenu {
    /// <summary>
    /// Controls the settings menu, currently only the close button. (Slider volumes are handled solely by SoundManager)
    /// </summary>
    public class VersusSetupMenu : MonoBehaviour
    {
        
        [SerializeField] private InputActionReference closeAction;

        [SerializeField] private Menu3d menu3D;

        private void OnEnable() {
            closeAction.action.performed += OnMenuClose;
        }

        private void OnDisable() {
            closeAction.action.performed -= OnMenuClose;
        }

        private void OnMenuClose(InputAction.CallbackContext ctx) {
            if (!enabled) return;
            if (!gameObject) return;
            if (!gameObject.activeSelf) return;
            menu3D.CloseVersus();
        }
    }
}
