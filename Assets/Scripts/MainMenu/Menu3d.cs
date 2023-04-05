using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    // p1 input script so that R to submit works in menu
    [SerializeField] private InputScript[] inputScripts;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (InputScript inputScript in inputScripts) {
            if (Input.GetKeyDown(inputScript.Cast)) {
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            }
        }
    }

    public void SelectVersus()
    {
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
}
