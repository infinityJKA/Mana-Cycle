using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

using Sound;

namespace Pause {
    public class PauseMenu : MonoBehaviour
    {

        public bool paused  { get; set; }
        [SerializeField] private GameObject PauseUI;
        [SerializeField] private Transform buttonsTransform;
        
        // Start is called before the first frame update
        void Start()
        {
            // unpause the game incase paused
            paused = true;
            TogglePause();

            // Change button text based on gamemode
            if (Storage.gamemode == Storage.GameMode.Versus) {
                buttonsTransform.Find("RetryButton").GetChild(0).GetComponent<TextMeshProUGUI>()
                    .text = "Rematch";
            }
            
            if (Storage.gamemode == Storage.GameMode.Versus || (Storage.level && Storage.level.availableBattlers.Count > 1)) {
                buttonsTransform.Find("LevelSelectButton").gameObject.SetActive(false);
                buttonsTransform.Find("CharSelectButton").gameObject.SetActive(true);
            }

            // dont allow char select option in the middle of arcade mode
            if (Storage.level && Storage.level.nextSeriesLevel) 
            {
                buttonsTransform.Find("LevelSelectButton").gameObject.SetActive(true);
                buttonsTransform.Find("CharSelectButton").gameObject.SetActive(false);
            }
        }

        public void TogglePause()
        {
            paused = !paused;
            if (paused)
            { 
                // game paused
                Time.timeScale = 0f; 
                SoundManager.Instance.PauseBGM();

                // when in solo mode, hide css button. when in multi, hide solo button.
                // only remove extra button if it still exists  
                // if (pauseMenuItems.Count > 3){
                    
                //     if (Storage.gamemode == Storage.GameMode.Solo)
                //     {
                //         pauseMenuItems[1].SetActive(false);
                //         pauseMenuItems.RemoveAt(1);
                //     }
                //     else
                //     {
                //         pauseMenuItems[2].SetActive(false);
                //         pauseMenuItems.RemoveAt(2);
                //     }
                // }

                // clear selected menu button
                EventSystem.current.SetSelectedGameObject(null);
                // set first selected button
                EventSystem.current.SetSelectedGameObject(buttonsTransform.GetChild(0).gameObject);

                PauseUI.SetActive(true);
            }
            else 
            {
                // game unpaused
                Time.timeScale = 1f;

                PauseUI.SetActive(false);

                SoundManager.Instance.UnpauseBGM();
            }
        }

        public void MoveCursor(Vector3 dir)
        {
            var toSelect = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectable(dir);
            if (toSelect != null) EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
        }

        public void SelectOption()
        {
            // Debug.Log(pauseMenuItems[currentSelection]);
            if (paused) SoundManager.Instance.UnpauseBGM();
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
        }
    }
}