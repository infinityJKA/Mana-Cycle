using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public bool paused  { get; private set; }
    [SerializeField] private GameObject PauseUI;
    [SerializeField] private List<GameObject> pauseMenuItems;
    private int currentSelection = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        paused = true;
        TogglePause();
    }

    public void TogglePause()
    {
        paused = !paused;
        if (paused)
        { 
            // game paused
            Time.timeScale = 0f; 
            PauseUI.SetActive(true);

            // clear selected menu button
            EventSystem.current.SetSelectedGameObject(null);
            // set first selected button
            currentSelection = 0;
            EventSystem.current.SetSelectedGameObject(pauseMenuItems[currentSelection]);

        }
        else 
        {
            // game unpaused
            Time.timeScale = 1f;
            PauseUI.SetActive(false);
        }
    }

    public void MoveCursor(int amount)
    {
        currentSelection += amount;
        currentSelection = Math.Abs(currentSelection) % pauseMenuItems.Count;
        EventSystem.current.SetSelectedGameObject(pauseMenuItems[currentSelection]);
    }

    public void SelectOption()
    {
        Debug.Log(pauseMenuItems[currentSelection]);
        pauseMenuItems[currentSelection].GetComponent<Button>().onClick.Invoke();
    }
}
