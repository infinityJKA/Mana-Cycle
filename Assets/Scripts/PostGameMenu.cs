using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

public class PostGameMenu : MonoBehaviour
{

    [SerializeField] private List<GameObject> MenuItems;
    private int currentSelection = 0;
    private double appearTime;
    private bool timerStarted = false;
    [SerializeField] private GameObject MenuUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timerStarted)
        {
            // timescale is currently 0 to pause game logic, so used unscaled dt
            appearTime -= Time.unscaledDeltaTime;

            if (appearTime <= 0)
            {
                MenuUI.SetActive(true);
                timerStarted = false;

                EventSystem.current.SetSelectedGameObject(null);
                MoveCursor(0);
            }
        }


    }

    public void AppearWithDelay(double s)
    {
        appearTime = s;
        timerStarted = true;
    }

    // alot of code is repeated from the pause menu script. could be cleaned up later
    public void MoveCursor(int amount)
    {
        currentSelection += amount;
        currentSelection = Utils.mod(currentSelection, MenuItems.Count);
        EventSystem.current.SetSelectedGameObject(MenuItems[currentSelection]);
    }

    public void SelectOption()
    {
        Debug.Log(MenuItems[currentSelection]);
        MenuItems[currentSelection].GetComponent<Button>().onClick.Invoke();
    }
}
