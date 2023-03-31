using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public bool paused  { get; set; }
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

            // when in solo mode, hide css button. when in multi, hide solo button.
            // only remove extra button if it still exists  
            if (pauseMenuItems.Count > 3){
                
                if (Storage.gamemode == Storage.GameMode.Solo)
                {
                    pauseMenuItems[2].SetActive(false);
                    pauseMenuItems.RemoveAt(2);
                }
                else
                {
                    pauseMenuItems[3].SetActive(false);
                    pauseMenuItems.RemoveAt(3);
                }
            }


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
        currentSelection -= amount;
        currentSelection = Utils.mod(currentSelection, pauseMenuItems.Count);
        EventSystem.current.SetSelectedGameObject(pauseMenuItems[currentSelection]);
    }

    public void SelectOption()
    {
        // Debug.Log(pauseMenuItems[currentSelection]);
        pauseMenuItems[currentSelection].GetComponent<Button>().onClick.Invoke();
    }
}
