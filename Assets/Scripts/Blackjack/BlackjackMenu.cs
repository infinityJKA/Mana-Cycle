using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;

namespace MainMenu {
    public class BlackjackMenu : MonoBehaviour
    {
        [SerializeField] GameObject BlackjackPlayMenu;
        [SerializeField] BlackjackGame BlackjackPlayScript;
        [SerializeField] GameObject MainWindow;

        [SerializeField] public GameObject BJfirstSelected;

        [SerializeField] private TransitionScript TransitionHandler;

        ///<summary>GameObject that holds all the Button objects in the scene</summary>
        [SerializeField] Transform buttonTransorm;

        [SerializeField] bool mobile;

        [SerializeField] private CinemachineVirtualCamera thisCam;
        [SerializeField] private CinemachineBrain brain;
        
        public event Action BlackjackCloseListener;

        public static bool blackjackOpened {get; private set;} = false;

        // [SerializeField] PlayerInput playerInput;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void OpenBlackjack()
        {
            blackjackOpened = true;
            gameObject.SetActive(true);

            StartCoroutine(SelectFirstButtonNextFrame());

            if (brain && brain.ActiveVirtualCamera != null) brain.ActiveVirtualCamera.Priority = 1;
            if (thisCam) thisCam.Priority = 30;
        }

        IEnumerator SelectFirstButtonNextFrame() {
            yield return new WaitForEndOfFrame();
            if (!mobile) EventSystem.current.SetSelectedGameObject(buttonTransorm.GetChild(0).gameObject);
        }

        public void ExitBlackjack()
        {
            blackjackOpened = false;
            gameObject.SetActive(false);
            BlackjackCloseListener?.Invoke();
        }

        // Update is called once per frame
        // Commented out because this is currently handled with unity's built in system, doing it in this script at the same time can double inputs
        // (Not noticable for the versus/solo buttons but is noticable for settings and HTP menus)

       public void OpenBlackjackPlayMenu()
        {
            BlackjackPlayMenu.SetActive(true);
            MainWindow.SetActive(false);
            if (!mobile) EventSystem.current.SetSelectedGameObject(BJfirstSelected);
        }
    }
}
