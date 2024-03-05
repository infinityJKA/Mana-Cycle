using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

using Sound;
using UnityEngine.Localization;

namespace Pause {
    public class PauseMenu : MonoBehaviour
    {

        public bool paused  { get; set; }
        [SerializeField] private GameObject PauseUI;
        [SerializeField] private Transform buttonsTransform;

        [SerializeField] private LocalizedString rematchLocalizedString;

        [SerializeField] private LocalizedString restartLocalizedString;

        [SerializeField] private bool mobile;
        
        // Start is called before the first frame update
        void Start()
        {
            // unpause the game incase paused
            paused = true;
            TogglePause();

            // Change button text based on gamemode - will become "rematch" in versus mode
            if (Storage.gamemode == Storage.GameMode.Versus) {
                buttonsTransform.Find("RetryButton").GetChild(0).GetComponent<TextMeshProUGUI>()
                    .text = rematchLocalizedString.GetLocalizedString();
            }
            
            if (Storage.gamemode == Storage.GameMode.Versus 
            || (Storage.level && Storage.level.availableBattlers != null && Storage.level.availableBattlers.Count > 1)) {
                buttonsTransform.Find("LevelSelectButton").gameObject.SetActive(false);
                buttonsTransform.Find("CharSelectButton").gameObject.SetActive(true);
            }

            // dont allow char select option in the middle of arcade mode
            if (Storage.level && Storage.level.nextSeriesLevel) 
            {
                buttonsTransform.Find("LevelSelectButton").gameObject.SetActive(true);
                buttonsTransform.Find("CharSelectButton").gameObject.SetActive(false);
            }

            if (Storage.level && (Storage.level.lastSeriesLevel || Storage.level.nextSeriesLevel))
            {
                // show "restart" instead of retry if this is a series of levels
                buttonsTransform.Find("RetryButton").GetChild(0).GetComponent<TextMeshProUGUI>()
                    .text = restartLocalizedString.GetLocalizedString();
            }

            if (Storage.level && Storage.level.generateNextLevel && Storage.level.lastSeriesLevel != null)
            {
                buttonsTransform.Find("RetryButton").gameObject.SetActive(false);
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

                if (!mobile) {
                    // clear selected menu button
                    EventSystem.current.SetSelectedGameObject(null);
                    // set first selected button
                    EventSystem.current.SetSelectedGameObject(buttonsTransform.GetChild(0).gameObject);
                }

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
            var button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            if (button) button.onClick.Invoke();
            if (paused) SoundManager.Instance.UnpauseBGM();
        }
    }
}