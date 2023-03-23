using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Menu3d : MonoBehaviour
{
    [SerializeField] private GameObject HTPWindow;
    [SerializeField] private HowToPlay HTPScript;
    [SerializeField] private GameObject MainWindow;

    [SerializeField] private GameObject MainFirstSelected;
    [SerializeField] private GameObject HTPFirstSelected;
    [SerializeField] private GameObject SettingsFirstSelected;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectVersus()
    {
        SceneManager.LoadScene("CharSelect");
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
        EventSystem.current.SetSelectedGameObject(MainFirstSelected);
    }

}
