using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Cinemachine;
using System.Collections.Generic;

namespace MainMenu {
    /// <summary>
    /// Controls the main menu buttons, the cinemachine camera targets in the 3dmenu & opens menus in the main menu.
    /// </summary>
    public class Menu3d : MonoBehaviour
    {
        public static Menu3d instance {get; private set;}

        [SerializeField] private GameObject HTPWindow;
        [SerializeField] private HowToPlay HTPScript;
        [SerializeField] public GameObject MainWindow;

        // [SerializeField] private GameObject MainFirstSelected;
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


        // things to hide in web builds, such as the quit button
        [SerializeField] GameObject[] hideInWebGL;

        private void Awake() {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            SelectLastSelected();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                foreach (GameObject o in hideInWebGL)
                {
                    o.SetActive(false);
                }
            }

            versionText.text = "v" + Application.version + " (" + Application.platform + ")";
        }

        public void SelectLastSelected()
        {
            if(mobile) return;

            if (SidebarUI.instance && SidebarUI.instance.expanded) {
                SidebarUI.instance.SelectLastSelected();
            } else {
                EventSystem.current.SetSelectedGameObject(buttonTransorm.GetChild(Storage.lastMainMenuItem).gameObject);
            }
        }

        public void SelectVersus()
        {
            VersusWindow.SetActive(true);
            MainWindow.SetActive(false);
            if (!mobile) EventSystem.current.SetSelectedGameObject(VersusFirstSelected);
        }

        public void CloseVersus()
        {
            VersusWindow.SetActive(false);
            MainWindow.SetActive(true);
            SelectLastSelected();
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
                case 1: Storage.online = false; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = false; break;
                case 2: Storage.online = false; Storage.isPlayerControlled1 = false; Storage.isPlayerControlled2 = false; break;
                case 3: Storage.online = true; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true; break;
                default: Storage.online = false; Storage.isPlayerControlled1 = true; Storage.isPlayerControlled2 = true; break;
            }
            TransitionHandler.WipeToScene("CharSelect");
        }

        public void GoToOnline() {
            Storage.online = true; 
            Storage.level = null;
            Storage.isPlayerControlled1 = true; 
            Storage.isPlayerControlled2 = true;
            Storage.gamemode = Storage.GameMode.Versus;
            TransitionHandler.WipeToScene("OnlineMenu");
        }

        public void SelectHTP()
        {
            if (BlackjackMenu.blackjackOpened) return;
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
            SelectLastSelected();
        }

        public void SelectSettings()
        {
            if (BlackjackMenu.blackjackOpened) return;
            SettingsWindow.SetActive(true);
            MainWindow.SetActive(false);
            if (!mobile) EventSystem.current.SetSelectedGameObject(SettingsFirstSelected);
            SettingsScript.Init();
        }

        public void SelectExtras()
        {
            if (BlackjackMenu.blackjackOpened) return;
            TransitionHandler.WipeToScene("ExtrasHub");
        }

        public void CloseSettings()
        {
            SettingsWindow.SetActive(false);
            MainWindow.SetActive(true);
            SelectLastSelected();
        }

        public void SelectSolo()
        {
            if (BlackjackMenu.blackjackOpened) return;
            Storage.online = false;
            TransitionHandler.WipeToScene("SoloMenu");
        }

        public void SelectExit()
        {
            Debug.Log("Quiting...");
            Application.Quit();
        }
    }
}
