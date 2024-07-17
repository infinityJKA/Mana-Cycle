using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using System.Collections;
using System;

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

        // todo move more settings to use PlayerPrefSetter script for modularity
        [SerializeField] private PlayerPrefSetter[] prefSetters;

        [SerializeField] private Menu3d menu3D;

        private void OnEnable() {
            closeAction.action.Enable();
            closeAction.action.performed += OnMenuClose;
            pauseAction.action.Enable();
            pauseAction.action.performed += OnMenuClose;

            Array.ForEach(prefSetters, pref => pref.Sync());
        }

        private void OnDisable() {
            closeAction.action.performed -= OnMenuClose;
            pauseAction.action.performed -= OnMenuClose;
        }

        private void OnMenuClose(InputAction.CallbackContext ctx) {
            if (gameObject == null) return;
            if (!gameObject) return;
            if (!gameObject.activeSelf) return;

            if (menu3D) menu3D.CloseSettings();
            else closeButton.onClick.Invoke();
        }

        void Start() {
            if (ghostPieceToggle) {
                ghostPieceToggle.isOn = Settings.current.drawGhostPiece;
            }

            // keep this in player prefs since it is platform specific
            windowModeDropdown.value = PlayerPrefs.GetInt("windowModeSelection");
        }

        void Update()
        {
            // if (Input.GetKeyDown(inputScript.Pause)) {
            //     closeButton.onClick.Invoke();
            // }
        }

        public void OnGhostPieceToggleChange() {
            Settings.current.drawGhostPiece = ghostPieceToggle.isOn;
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

            // This does not use the file-based player prefs, just normal playerpref so it doesn't get sent over steam cloud or anything 
            // and be used on different devices which may have different screen setup.
            PlayerPrefs.SetInt("windowModeSelection", selection);
            EventSystem.current.SetSelectedGameObject(windowModeDropdown.gameObject);
        }

        private Coroutine changeLanguageCoroutine;
        private string[] localeList = {"en", "ja"};
        public void ChangeLocale(int localeIndex) {
            if (changeLanguageCoroutine != null) StopCoroutine(changeLanguageCoroutine);
            changeLanguageCoroutine = StartCoroutine(SetLocale(localeList[localeIndex]));
        }

        IEnumerator SetLocale(string localeCode) {
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        }
    }
}
