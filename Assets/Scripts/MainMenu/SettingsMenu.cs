using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace MainMenu {
    /// <summary>
    /// Controls the settings menu, currently only the close button. (Slider volumes are handled solely by SoundManager)
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private InputScript inputScript;
        [SerializeField] private Toggle ghostPieceToggle;
        [SerializeField] private Button closeButton;
        
        [SerializeField] private InputActionReference closeAction, pauseAction;

        [SerializeField] private TMP_Dropdown windowModeDropdown;

        [SerializeField] private Menu3d menu3D;

        private void OnEnable() {
            closeAction.action.Enable();
            closeAction.action.performed += OnMenuClose;
            pauseAction.action.Enable();
            pauseAction.action.performed += OnMenuClose;
        }

        private void OnDisable() {
            closeAction.action.performed -= OnMenuClose;
            pauseAction.action.performed -= OnMenuClose;
        }

        private void OnMenuClose(InputAction.CallbackContext ctx) {
            if (gameObject == null) return;
            if (!gameObject) return;
            if (!gameObject.activeSelf) return;
            menu3D.CloseSettings();
        }

        void Start() {
            if (ghostPieceToggle) {
                if (!PlayerPrefs.HasKey("drawGhostPiece")) PlayerPrefs.SetInt("drawGhostPiece", 1);
                ghostPieceToggle.isOn = PlayerPrefs.GetInt("drawGhostPiece") == 1;
            }

            windowModeDropdown.value = PlayerPrefs.GetInt("windowModeSelection");
        }

        void Update()
        {
            // if (Input.GetKeyDown(inputScript.Pause)) {
            //     closeButton.onClick.Invoke();
            // }
        }

        public void OnGhostPieceToggleChange() {
            bool tickOn = ghostPieceToggle.isOn;
            PlayerPrefs.SetInt("drawGhostPiece", tickOn ? 1 : 0);
        }

        public void OnWindowModeChange()
        {
            int selection = windowModeDropdown.value;
            switch (windowModeDropdown.options[selection].text)
            {
                case "Borderless Fullscreen": 
                    Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                    break;
                case "Exclusive Fullscreen": 
                    Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen);
                    break;
                case "Resizable Window":
                    Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.Windowed);
                    break;
            }

            PlayerPrefs.SetInt("windowModeSelection", selection);
            EventSystem.current.SetSelectedGameObject(windowModeDropdown.gameObject);
        }

        
    }
}
