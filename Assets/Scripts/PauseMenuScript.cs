using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour
{

    public static bool paused = false;
    // [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject InputObj_P1;
    [SerializeField] private GameObject InputObj_P2;
    private InputScript inputScript_P1;
    private InputScript inputScript_P2; 
    [SerializeField] private GameObject PauseUI;
    [SerializeField] private List<GameObject> pauseMenuItems;
    private int currentSelection = 0;

    public void togglePause()
    {
            paused = (!paused);
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

    // Start is called before the first frame update
    void Start()
    {
        paused = true;
        togglePause();
        inputScript_P1 = InputObj_P1.GetComponent<InputScript>();
        inputScript_P2 = InputObj_P2.GetComponent<InputScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(inputScript_P1.Pause) || Input.GetKeyDown(inputScript_P2.Pause))
        {
            togglePause();
        }

        // menu nav
        if (paused){
            if (Input.GetKeyDown(inputScript_P1.Down)){
                currentSelection++;
                
            }
            if (Input.GetKeyDown(inputScript_P1.Up)){
                currentSelection--;
            }
            if (Input.GetKeyDown(inputScript_P1.Cast)){
                Debug.Log(pauseMenuItems[currentSelection]);
                (pauseMenuItems[currentSelection]).GetComponent<Button>().onClick.Invoke();
            }
            // make sure list selection doesnt go out of bounds
            currentSelection = Math.Abs(currentSelection) % pauseMenuItems.Count;
            EventSystem.current.SetSelectedGameObject(pauseMenuItems[currentSelection]);

        }
    }


}
