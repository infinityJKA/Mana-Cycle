using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private List<InputScript> inputScripts;
    [SerializeField] private List<GameObject> menuItems;
    private int currentSelection = 0;

    void Update(){
        foreach (InputScript inputScript in inputScripts)
        {
            if (Input.GetKeyDown(inputScript.Down)){
                currentSelection++;   
            }

            if (Input.GetKeyDown(inputScript.Up)){
                currentSelection--;
            }

            if (Input.GetKeyDown(inputScript.Cast)){
                Debug.Log(menuItems[currentSelection]);
                (menuItems[currentSelection]).GetComponent<Button>().onClick.Invoke();
            }
                
            // make sure list selection doesnt go out of bounds
            currentSelection = Math.Abs(currentSelection) % menuItems.Count;
            EventSystem.current.SetSelectedGameObject(menuItems[currentSelection]);
        }
    }

    public void OnPressStart()
    {
        Debug.Log("Pressed start");
        SceneManager.LoadScene("ManaCycle");
    }
}