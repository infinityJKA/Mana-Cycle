using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace MainMenu {
    /// <summary>
    /// Controls the main menu buttons, the cinemachine camera targets in the 3dmenu & opens menus in the main menu.
    /// </summary>
    public class Menu3d : MonoBehaviour
    {
        [SerializeField] private GameObject HTPWindow;
        [SerializeField] private HowToPlay HTPScript;
        [SerializeField] private GameObject MainWindow;

        [SerializeField] private GameObject MainFirstSelected;
        [SerializeField] private GameObject HTPFirstSelected;

        [SerializeField] private GameObject SettingsWindow;
        [SerializeField] private HowToPlay SettingsScript;
        [SerializeField] private GameObject SettingsFirstSelected;

        [SerializeField] private TransitionScript TransitionHandler;

        [SerializeField] private GameObject HTPButton, SettingsButton;

        [SerializeField] private GameObject VersusWindow, VersusButton, VersusFirstSelected;

        [SerializeField] private TMPro.TextMeshProUGUI versusDescription;

        // p1 input script so that R to submit works in menu
        [SerializeField] private InputScript[] inputScripts;

        [SerializeField] private TMPro.TextMeshProUGUI tipText;

        // Start is called before the first frame update
        void Start()
        {
            UpdateTip();
        }

        // Update is called once per frame
        void Update()
        {
            foreach (InputScript inputScript in inputScripts) {
                if (Input.GetKeyDown(inputScript.Cast)) {
                    if (EventSystem.current.currentSelectedGameObject != null) EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
                }
            }
        }

        public void SelectVersus()
        {
            VersusWindow.SetActive(true);
            MainWindow.SetActive(false);
            EventSystem.current.SetSelectedGameObject(VersusFirstSelected);
        }

        public void CloseVersus()
        {
            VersusWindow.SetActive(false);
            MainWindow.SetActive(true);
            EventSystem.current.SetSelectedGameObject(VersusButton);
        }

        public void setVersusDescription(string newText)
        {
            versusDescription.text = newText;
        }

        public void GoToCharSelect(int setup){
            // this is currently always called from main menu versus setup, so set gamemode to versus
            Storage.gamemode = Storage.GameMode.Versus;
            switch(setup){
                case 1: Storage.isPlayer1 = true; Storage.isPlayer2 = false; break;
                case 2: Storage.isPlayer1 = false; Storage.isPlayer2 = false; break;
                default: Storage.isPlayer1 = true; Storage.isPlayer2 = true; break;
            }
            TransitionHandler.WipeToScene("CharSelect");
        }

        public void SelectHTP()
        {
            HTPWindow.SetActive(true);
            MainWindow.SetActive(false);
            EventSystem.current.SetSelectedGameObject(HTPFirstSelected);
            HTPScript.Init();
            // HTPPage = 0;
        }

        public void CloseHTP()
        {
            HTPWindow.SetActive(false);
            MainWindow.SetActive(true);
            EventSystem.current.SetSelectedGameObject(HTPButton);
        }

        public void SelectSettings()
        {
            SettingsWindow.SetActive(true);
            MainWindow.SetActive(false);
            EventSystem.current.SetSelectedGameObject(SettingsFirstSelected);
            SettingsScript.Init();
        }

        public void CloseSettings()
        {
            SettingsWindow.SetActive(false);
            MainWindow.SetActive(true);
            EventSystem.current.SetSelectedGameObject(SettingsButton);
        }

        public void SelectSolo()
        {
            TransitionHandler.WipeToScene("SoloMenu");
        }

        public void UpdateTip()
        {
            tipText.text = String.Format("[Arrow Keys] to move cursor\n[{0}] to select", inputScripts[0].Cast);
        }
    }
}
