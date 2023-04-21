using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MainMenu {
    /** tbh, idk why this script is even here, seems like 3dmenu script does its job */
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private List<InputScript> inputScripts;
        [SerializeField] private List<GameObject> menuItems;
        // unity can't serialize 2d arrays so we have to do it goofy style
        [SerializeField] private int columns = 2;
        private int rows;
        private int currentRow = 0;
        private int currentCol = 0;
        private int currentSelection = 0;

        void Start(){
            rows = menuItems.Count / columns;
        }

        void Update(){
            foreach (InputScript inputScript in inputScripts)
            {
                if (Input.GetKeyDown(inputScript.Right)){
                    currentCol = Math.Min(currentCol + 1, columns-1);   
                }

                if (Input.GetKeyDown(inputScript.Left)){
                    currentCol = Math.Max(currentCol - 1, 0);   
                }

                if (Input.GetKeyDown(inputScript.Up)){
                    currentRow = Math.Max(currentRow - 1, 0);   
                }

                if (Input.GetKeyDown(inputScript.Down)){
                    currentRow = Math.Min(currentRow + 1, rows-1);   
                }

                if (Input.GetKeyDown(inputScript.Cast)){
                    Debug.Log(menuItems[currentSelection]);
                    (menuItems[currentSelection]).GetComponent<Button>().onClick.Invoke();
                }
                    
                // set row and col to correct item
                currentSelection = currentCol + currentRow*columns;
                // Debug.Log(currentCol + ", " + currentRow + "  |  " + currentSelection);
                EventSystem.current.SetSelectedGameObject(menuItems[currentSelection]);
            }
        }

        public void OnPressStart()
        {
            // Debug.Log("Pressed start");
            SceneManager.LoadScene("CharSelect");
        }
    }
}