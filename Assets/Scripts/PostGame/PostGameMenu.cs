using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

using Battle.Board;
using Sound;

namespace PostGame {
    public class PostGameMenu : MonoBehaviour
    {

        [SerializeField] private Transform buttonsTransform;
        

        // ---- Timer
        private double appearTime;
        public bool timerRunning {get; private set;}
        [SerializeField] private GameObject MenuUI;

        private TransitionScript transitionHandler;

        /** player 1 if solo mode, otherwise winning board in versus */
        private GameBoard board;

        // if this post game menu has been displayed
        private bool displayed = false;

        // Rematch button - text changed to "retry" in solo mode
        public Button retryButton;

        [SerializeField] private AudioClip defeatMusic;
        [SerializeField] private AudioClip winMusic;
        [SerializeField] private AudioClip moveSFX;
        [SerializeField] private AudioClip selectSFX;

        
        // arcade mode info, hidden outside of arcade mode
        [SerializeField] private GameObject arcadeInfoPannel;
        [SerializeField] private TMPro.TextMeshProUGUI arcadeInfoText;

        // Start is called before the first frame update
        void Start()
        {
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
            timerRunning = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (timerRunning)
            {
                // timescale is currently 0 to pause game logic, so used unscaled dt
                appearTime -= Time.unscaledDeltaTime;

                // wait for spellcasts and convos to finish
                if (appearTime <= 0 && !board.IsCasting() && !board.convoPaused && board.isPostGame())
                {
                    // Play convos if amy are remaining
                    bool convoPlayed = board.CheckMidLevelConversations();
                    if (convoPlayed) return;

                    DisplayPostGameGUI();
                }

                Storage.convoEndedThisInput = false;
            }
        }

        void DisplayPostGameGUI() {
            displayed = true;
            timerRunning = false;

            // will be set active if in arcade mode & won
            arcadeInfoPannel.SetActive(false);
            
            // solo mode in series: retry -> continue

            if (Storage.gamemode == Storage.GameMode.Versus || (Storage.level && Storage.level.availableBattlers.Count > 1))
            {
                buttonsTransform.Find("LevelSelectButton").gameObject.SetActive(false);
                buttonsTransform.Find("CharSelectButton").gameObject.SetActive(true);
            }

            // Update level clear status
            if (Storage.gamemode == Storage.GameMode.Solo)
            {
                string levelID = Storage.level.levelName;

                // if not endless mode and is winner, level is cleared
                bool clearedBefore = PlayerPrefs.GetInt(levelID+"_Cleared", 0) == 1;
                bool cleared = board.IsWinner() || (Storage.level.time == -1 && Storage.level.scoreGoal == 0);
                if (cleared) PlayerPrefs.SetInt(levelID+"_Cleared", 1);

                // If solo mode win: retry -> replay
                retryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>()
                    .text = "Replay";

                // set highscore if level was cleared
                if (cleared) {
                    int highScore = PlayerPrefs.GetInt(levelID+"_HighScore", 0);
                    PlayerPrefs.SetInt(levelID+"_HighScore", Math.Max(board.hp, highScore));
                }

                Time.timeScale = 1f;
                
                // if first clear (and not in series), immediately exit back to solomenu; otherwise, open menu
                if (!clearedBefore && cleared && Storage.level.nextSeriesLevel == null) {
                    transitionHandler.WipeToScene("SoloMenu", reverse:true);
                    setMenuSong();
                } 
                else
                {
                    SoundManager.Instance.SetBGM(cleared ? winMusic : defeatMusic);
                    MenuUI.SetActive(true);
                    Time.timeScale = 0f;

                    // dont allow char select option in the middle of arcade mode
                    if (Storage.level.nextSeriesLevel) 
                    {
                        buttonsTransform.Find("LevelSelectButton").gameObject.SetActive(true);
                        buttonsTransform.Find("CharSelectButton").gameObject.SetActive(false);
                    }
                    

                    // if in level series, replay button -> continue button
                    if (Storage.level.nextSeriesLevel && cleared)
                    {
                        retryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Continue";
                        var ev = new Button.ButtonClickedEvent();
                        ev.AddListener(() => SelectContinue());
                        retryButton.onClick = ev;

                        buttonsTransform.Find("LevelSelectButton").gameObject.SetActive(true);
                        buttonsTransform.Find("CharSelectButton").gameObject.SetActive(false);
                        

                        // set info pannel visibility and text
                        arcadeInfoPannel.SetActive(true);
                        arcadeInfoText.text = String.Format("{0} more to go\nnext up: {1}", Storage.level.GetAheadCount(), Storage.level.nextSeriesLevel.levelName);
                    }
                }

                // if reached the end of a solo level, go to win screen
                if (Storage.level.nextSeriesLevel == null && Storage.level.lastSeriesLevel != null)
                {
                    Time.timeScale = 1f;
                    MenuUI.SetActive(false);
                    transitionHandler.WipeToScene("ArcadeWin");
                } 

                // Debug.Log("why did this stop working :((");
                // Debug.Log(Storage.level.nextSeriesLevel);
                // Debug.Log(Storage.level.lastSeriesLevel);
            }

            else
            {
                // In versus mode: retry -> rematch
                retryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>()
                    .text = "Rematch";

                MenuUI.SetActive(true);
                Time.timeScale = 0f;
                // when in multi, disable solo button and continue button
                // MenuItems[2].SetActive(false);
                // MenuItems.RemoveAt(2);
                // MenuItems[2].SetActive(false);
                // MenuItems.RemoveAt(2);

                SoundManager.Instance.SetBGM(winMusic);

                // rematchTextGUI.text = "Rematch";
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttonsTransform.GetChild(0).gameObject);
        }

        public void AppearAfterDelay(GameBoard board)
        {
            if (timerRunning || displayed) return;
            this.board = board;
            appearTime = 3d;
            timerRunning = true;
        }

        // // alot of code is repeated from the pause menu script. could be cleaned up later
        // public void MoveCursor(int amount)
        // {
        //     currentSelection -= amount;
        //     currentSelection = Utils.mod(currentSelection, MenuItems.Count);
        //     EventSystem.current.SetSelectedGameObject(MenuItems[currentSelection]);
        //     SoundManager.Instance.PlaySound(moveSFX, pitch : 0.75f);
        // }

        // public void SelectOption()
        // {
        //     if (Storage.convoEndedThisInput) return;
        //     // Debug.Log(MenuItems[currentSelection]);
        //     MenuItems[currentSelection].GetComponent<Button>().onClick.Invoke();
        //     SoundManager.Instance.PlaySound(selectSFX);
        // }

        public void MoveCursor(Vector3 dir)
        {
            var toSelect = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectable(dir);
            if (toSelect != null) EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
        }

        public void SelectOption()
        {
            // Debug.Log(pauseMenuItems[currentSelection]);
            if (Storage.convoEndedThisInput) return;
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            SoundManager.Instance.PlaySound(selectSFX);
        }

        public void SelectRematchButton()
        {
            SceneManager.LoadScene("ManaCycle");
        }

        public void SelectBackToMainButton()
        {
            setMenuSong();
            Time.timeScale = 1f;
            transitionHandler.WipeToScene("MainMenu", reverse: true);
        }

        public void SelectBackToCSS()
        {
            setMenuSong();
            Time.timeScale = 1f;
            transitionHandler.WipeToScene("CharSelect", reverse: true);
        }

        public void SelectBackToSolo()
        {
            setMenuSong();
            Time.timeScale = 1f;
            transitionHandler.WipeToScene("SoloMenu", reverse: true);
        }

        public void setMenuSong(){
            SoundManager.Instance.SetBGM(SoundManager.Instance.mainMenuMusic);
        }

        public void SelectContinue()
        {
            Time.timeScale = 1f;
            Storage.level.nextSeriesLevel.battler = Storage.level.battler;
            Storage.level = Storage.level.nextSeriesLevel;
            Storage.lives = board.lives;
            transitionHandler.WipeToScene("ManaCycle");
        }


    }
}
