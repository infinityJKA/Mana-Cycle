using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuScript : MonoBehaviour
{

    [SerializeField] private GameObject InputObj;
    private InputScript inputScript;
    [SerializeField] private List<GameObject> MenuItems;
    private int currentSelection = 0;

    void Start(){
        inputScript = InputObj.GetComponent<InputScript>();
    }

    void Update(){
        if (Input.GetKeyDown(inputScript.Down)){
            currentSelection++;   
            }

        if (Input.GetKeyDown(inputScript.Up)){
                currentSelection--;
            }

        if (Input.GetKeyDown(inputScript.Cast)){
                Debug.Log(MenuItems[currentSelection]);
                (MenuItems[currentSelection]).GetComponent<Button>().onClick.Invoke();
            }
            
        // make sure list selection doesnt go out of bounds
        currentSelection = Math.Abs(currentSelection) % MenuItems.Count;
        EventSystem.current.SetSelectedGameObject(MenuItems[currentSelection]);
    }

}
