using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;

namespace MainMenu {
    public class BlackjackMenu : MonoBehaviour
    {
        [SerializeField] GameObject BlackjackPlayMenu;
        [SerializeField] BlackjackGame BlackjackPlayScript;
        [SerializeField] GameObject MainWindow;

        [SerializeField] GameObject MainFirstSelected;

        [SerializeField] GameObject BJfirstSelected;

        [SerializeField] private TransitionScript TransitionHandler;

        ///<summary>GameObject that holds all the Button objects in the scene</summary>
        [SerializeField] Transform buttonTransorm;

        [SerializeField] bool mobile;

        // [SerializeField] PlayerInput playerInput;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        // Commented out because this is currently handled with unity's built in system, doing it in this script at the same time can double inputs
        // (Not noticable for the versus/solo buttons but is noticable for settings and HTP menus)

       public void OpenBlackjackPlayMenu()
        {
            BlackjackPlayMenu.SetActive(true);
            MainWindow.SetActive(false);
            Storage.lastMainMenuItem = 0;
            if (!mobile) EventSystem.current.SetSelectedGameObject(BJfirstSelected);
        }


        public void ExitBlackjack (int setup){
            TransitionHandler.WipeToScene("MainMenu");
        }


    }
}