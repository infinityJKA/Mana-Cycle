using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Cinemachine;

namespace MainMenu {
    /// <summary>
    /// Controls the main menu buttons, the cinemachine camera targets in the 3dmenu & opens menus in the main menu.
    /// </summary>
    public class Menu3d : MonoBehaviour
    {
        [SerializeField] private GameObject HTPWindow;
        [SerializeField] private HowToPlay HTPScript;
        [SerializeField] public GameObject MainWindow;

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

        ///<summary>GameObject that holds all the Button objects in the scene</summary>
        [SerializeField] private Transform buttonTransorm;

        // Text to dispalys the current version
        [SerializeField] private TMPro.TextMeshProUGUI versionText;

        [SerializeField] private bool mobile;


        // Blackjack canvas, easter egg in main menu
        [SerializeField] private BlackjackMenu blackjackMenu;


        // Start is called before the first frame update
        void Start()
        {
            SelectLastSelected();
        }

        public void SelectLastSelected()
        {
            if(!mobile) EventSystem.current.SetSelectedGameObject(buttonTransorm.GetChild(Storage.lastMainMenuItem).gameObject);

            UpdateTip();

            versionText.text = "v" + Application.version;
        }

        // Update is called once per frame
        // not sure why this was commented out
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                blackjackMenu.OpenBlackjack();
            }
        }

        public void SelectVersus()
        {
            VersusWindow.SetActive(true);
            MainWindow.SetActive(false);
            Storage.lastMainMenuItem = 2;
            if (!mobile) EventSystem.current.SetSelectedGameObject(VersusFirstSelected);
        }

        public void CloseVersus()
        {
            VersusWindow.SetActive(false);
            MainWindow.SetActive(true);
            if (!mobile) EventSystem.current.SetSelectedGameObject(VersusButton);
        }

        public void setVersusDescription(string newText)
        {
            versusDescription.text = newText;
        }

        public void GoToCharSelect(int setup){
            // this is currently always called from main menu versus setup, so set gamemode to versus
            Storage.gamemode = Storage.GameMode.Versus;
            Storage.level = null;
            switch(setup){
                case 1: Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = false; break;
                case 2: Storage.isPlayerControlled1 = false; Storage.isPlayerControlled2 = false; break;
                default: Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true; break;
            }
            TransitionHandler.WipeToScene("CharSelect");
        }

        public void SelectHTP()
        {
            HTPWindow.SetActive(true);
            MainWindow.SetActive(false);
            if (!mobile) EventSystem.current.SetSelectedGameObject(HTPFirstSelected);
            HTPScript.Init();
            // HTPPage = 0;
        }

        public void CloseHTP()
        {
            HTPWindow.SetActive(false);
            MainWindow.SetActive(true);
            if (!mobile) EventSystem.current.SetSelectedGameObject(HTPButton);
        }

        public void SelectSettings()
        {
            SettingsWindow.SetActive(true);
            MainWindow.SetActive(false);
            if (!mobile) EventSystem.current.SetSelectedGameObject(SettingsFirstSelected);
            SettingsScript.Init();
        }

        public void CloseSettings()
        {
            SettingsWindow.SetActive(false);
            MainWindow.SetActive(true);
            if (!mobile) EventSystem.current.SetSelectedGameObject(SettingsButton);
            UpdateTip();
        }

        public void SelectSolo()
        {
            Storage.lastMainMenuItem = 1;
            TransitionHandler.WipeToScene("SoloMenu");
        }

        public void UpdateTip()
        {
            tipText.text = String.Format("[Arrow Keys] to move cursor\n[{0}] to select", Utils.KeySymbol(inputScripts[0].Cast));
        }
    }
}
