using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using Random=UnityEngine.Random;

using Battle.Board;
using Battle.Cycle;
using Sound;
using SoloMode;
using Achievements;
using UnityEngine.Localization;

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
        [SerializeField] private GameBoard board;

        // if this post game menu has been displayed
        private bool displayed = false;

        // Rematch button - text changed to "retry" in solo mode
        public Button retryButton, continueButton, charSelectButton, levelSelectButton;

        [SerializeField] private AudioClip defeatMusic;
        [SerializeField] private AudioClip winMusic;
        [SerializeField] private AudioClip moveSFX;
        [SerializeField] private AudioClip selectSFX;

        
        // arcade mode info, hidden outside of arcade mode
        [SerializeField] private GameObject arcadeInfoPannel;
        [SerializeField] private TMPro.TextMeshProUGUI arcadeInfoText;

        [SerializeField] private LevelGenerator levelGenerator;

        [SerializeField] private LocalizedString rematchLocalizedString;
        [SerializeField] private LocalizedString replayLocalizedString;
        [SerializeField] private LocalizedString restartLocalizedString;

        // online vars for the names that show player postgameintentions
        [SerializeField] private TMP_Text p1Intention, p2Intention;
        [SerializeField] private Transform p1DecidingPosition, p2DecidingPosition;
        [SerializeField] private Color decidingColor, decideColor;
        [SerializeField] private float intentionOffset = 25f;

        // Start is called before the first frame update
        void Start()
        {
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
            timerRunning = false;
            continueButton.gameObject.SetActive(false);
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

            if (Storage.gamemode == Storage.GameMode.Versus || (Storage.level != null && Storage.level.availableBattlers != null && Storage.level.availableBattlers.Count > 1))
            {
                levelSelectButton.gameObject.SetActive(false);
                charSelectButton.gameObject.SetActive(true);
            }

            // Update level clear status
            if (Storage.gamemode == Storage.GameMode.Solo)
            {
                string levelID = Storage.level.levelName;

                // if not endless mode and is winner, level is cleared
                bool clearedBefore = PlayerPrefs.GetInt(levelID+"_Cleared", 0) == 1;
                bool cleared = board.IsWinner() || (Storage.level.time == -1 && Storage.level.scoreGoal == 0);

                // set highscore if level was cleared
                if (cleared) {        
                    PlayerPrefs.SetInt(levelID+"_Cleared", 1);

                    int score = Storage.level.IsEndless() ? board.hp : board.hp + (board.lives-1)*2000; // add 2000 to score for each remaining life
                    int highScore = PlayerPrefs.GetInt(levelID+"_HighScore", 0);
                    PlayerPrefs.SetInt(levelID+"_HighScore", Math.Max(score, highScore));

                    // If solo mode win: retry -> replay
                    retryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>()
                        .text = replayLocalizedString.GetLocalizedString();
                }

                Time.timeScale = 1f;
                
                // if first clear (and not in series or endless), immediately exit back to solomenu; otherwise, open menu
                if (!clearedBefore && cleared && Storage.level.nextSeriesLevel == null && Storage.level.time != -1 && !Storage.level.generateNextLevel) {
                    // if reached the end of a solo level, go to win screen
                    if (Storage.level.nextSeriesLevel == null && Storage.level.lastSeriesLevel != null && !Storage.level.generateNextLevel)
                    {
                        transitionHandler.WipeToScene("ArcadeWin");
                    } else
                    {
                        transitionHandler.WipeToScene("SoloMenu", reverse: true);
                        setMenuSong();
                    }
                } 
                else
                {
                    SoundManager.Instance.SetBGM(cleared ? winMusic : defeatMusic);
                    MenuUI.SetActive(true);
                    Time.timeScale = 0f;

                    // dont allow char select option in the middle of arcade mode
                    if (Storage.level.nextSeriesLevel) 
                    {
                        levelSelectButton.gameObject.SetActive(true);
                        charSelectButton.gameObject.SetActive(false);
                    }
                    

                    // if in level series, replay button -> continue button
                    if (Storage.level.nextSeriesLevel && cleared && !Storage.level.generateNextLevel)
                    {
                        retryButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);

                        levelSelectButton.gameObject.SetActive(true);
                        charSelectButton.gameObject.SetActive(false);
                        

                        // set info pannel visibility and text
                        arcadeInfoPannel.SetActive(true);
                        arcadeInfoText.text = string.Format("{0} more to go\nnext up: {1}", Storage.level.GetAheadCount(), Storage.level.nextSeriesLevel.levelName);
                    }

                    // arcade endless level won
                    if (Storage.level.generateNextLevel && cleared)
                    {
                        Debug.Log("Curious");
                        retryButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);

                        levelSelectButton.gameObject.SetActive(true);
                        charSelectButton.gameObject.SetActive(false);

                        // setup next level list for arcade endless scene
                        Storage.nextLevelChoices = new List<Level>();
                        // add 3 levels with difficutly based on current match in AE. add between 0-0.1 to difficulty for variety
                        for (int i = 0; i < 3; i++)
                        {
                            Storage.nextLevelChoices.Add(levelGenerator.Generate(
                            difficulty: ((Storage.level.GetBehindCount() + 1) * 0.05f) + (i * 0.05f) + 0.05f,
                            battler: Storage.level.battler,
                            lastLevel: Storage.level));
                        }
                        Utils.Shuffle(Storage.nextLevelChoices);

                        // add reward of level to total
                        ArcadeStats.moneyAmount += (int) (Storage.level.rewardAmount * ArcadeStats.playerStats[ArcadeStats.Stat.MoneyMult]);
                        if (Storage.level.itemReward != null) Inventory.ObtainItem(Storage.level.itemReward); 
                    }
                }

                // if reached the end of a solo level, go to win screen
                if (Storage.level.nextSeriesLevel == null && Storage.level.lastSeriesLevel != null && !Storage.level.generateNextLevel)
                {
                    Time.timeScale = 1f;
                    MenuUI.SetActive(false);
                    transitionHandler.WipeToScene("ArcadeWin");
                }

                // Debug.Log("why did this stop working :((");
                // Debug.Log(Storage.level.nextSeriesLevel);
                // Debug.Log(Storage.level.lastSeriesLevel);
            }

            else if (Storage.gamemode == Storage.GameMode.Versus)
            {
                // In versus mode: retry -> rematch
                retryButton.transform.GetComponentInChildren<TextMeshProUGUI>()
                    .text = rematchLocalizedString.GetLocalizedString();

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

            AchievementHandler achievementHandler = FindAnyObjectByType<AchievementHandler>();
            if (board.IsPlayerControlled()) achievementHandler.CheckAchievements(board);
            if (!Storage.online && board.enemyBoard.IsPlayerControlled()) achievementHandler.CheckAchievements(board.enemyBoard);

            // if (!board.Mobile) {
            //     EventSystem.current.SetSelectedGameObject(null);
            //     EventSystem.current.SetSelectedGameObject(buttonsTransform.GetChild(0).gameObject);
            // }

            if (retryButton.gameObject.activeInHierarchy) {
                retryButton.Select();
            } else {
                continueButton.Select();
            }

            p1Intention.gameObject.SetActive(Storage.online);
            p2Intention.gameObject.SetActive(Storage.online);

            if (Storage.online) {
                SetIntention(NetPlayer.PostGameIntention.Undecided, true);
                SetIntention(NetPlayer.PostGameIntention.Undecided, false);
            }
        }

        public void AppearAfterDelay()
        {
            if (timerRunning || displayed) return;
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
            if (toSelect != null){
                EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
                Instantiate(moveSFX);
            }
        }

        public void SelectOption()
        {
            // Debug.Log(pauseMenuItems[currentSelection]);
            if (Storage.convoEndedThisInput) return;
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            Instantiate(selectSFX);
        }

        public void SelectRematchButton()
        {
            if (Storage.level && Storage.level.lastSeriesLevel != null)
            {
                Storage.level = Storage.level.GetRootLevel();
                Storage.lives = Storage.level.lives;
                transitionHandler.WipeToScene("ManaCycle", reverse: true);
                Time.timeScale = 1f;
            }
            else
            {
                if (Storage.online) {
                    // Toggle rematch requested status when in online mode
                    if (board.netPlayer.postGameIntention == NetPlayer.PostGameIntention.Rematch) {
                        board.netPlayer.CmdSetPostGameIntention(NetPlayer.PostGameIntention.Undecided);
                    } else {
                        board.netPlayer.CmdSetPostGameIntention(NetPlayer.PostGameIntention.Rematch);
                    }
                } else {
                    // restart immediately in local play
                    Replay();
                }
            }
        }

        public static void Replay() {
            ManaCycle.initializeFinished = false;
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
            if (Storage.online) {
                if (board.netPlayer.postGameIntention == NetPlayer.PostGameIntention.CharSelect) {
                    board.netPlayer.CmdSetPostGameIntention(NetPlayer.PostGameIntention.Undecided);
                } else {
                    board.netPlayer.CmdSetPostGameIntention(NetPlayer.PostGameIntention.CharSelect);
                }
            } else {
                BackToCSS();
            }
        }

        public static void BackToCSS() {
            setMenuSong();
            Time.timeScale = 1f;
            TransitionScript.instance.WipeToScene("CharSelect", reverse: true);
        }

        public void OnMainMenuDown() {
            Debug.Log("down");
        }

        public void OnMainMenuUp() {
            Debug.Log("up");
        }

        public void SetIntention(NetPlayer.PostGameIntention intention, bool isPlayer1) {
            TMP_Text intentionLabel = isPlayer1 ? p1Intention : p2Intention;
            Transform intendedButtonTransform;
            int boardIndex = isPlayer1 ? 0 : 1;
            string username = ManaCycle.instance.Boards[boardIndex].netPlayer.username;

            switch (intention) {
                case NetPlayer.PostGameIntention.Undecided:
                    intentionLabel.transform.position = (isPlayer1 ? p1DecidingPosition : p2DecidingPosition).position;
                    intentionLabel.text = username+"\nDeciding...";
                    intentionLabel.color = decidingColor;
                    return;
                case NetPlayer.PostGameIntention.Rematch:
                    intendedButtonTransform = retryButton.transform;
                    break;
                default: // charselect
                    intendedButtonTransform = charSelectButton.transform;
                    break;
            }

            intentionLabel.transform.position = intendedButtonTransform.position + (isPlayer1 ? Vector3.left : Vector3.right) * intentionOffset;
            intentionLabel.text = isPlayer1 ? username+" >" : "< "+username;
            intentionLabel.color = decideColor;
        }

        public void SelectBackToSolo()
        {
            setMenuSong();
            Time.timeScale = 1f;
            transitionHandler.WipeToScene("SoloMenu", reverse: true);
        }

        public static void setMenuSong(){
            SoundManager.Instance.SetBGM(SoundManager.Instance.mainMenuMusic);
        }

        public void SelectContinue()
        {
            Time.timeScale = 1f;
            Storage.lives = board.recoveryMode ? 2000 : board.lives;
            Storage.hp = board.hp;
            if (Storage.level.generateNextLevel) 
            {
                transitionHandler.WipeToScene("SelectNextLevel");
            }
            else
            {
                Storage.level.nextSeriesLevel.battler = Storage.level.battler;
                Storage.level = Storage.level.nextSeriesLevel; 
                transitionHandler.WipeToScene("ManaCycle");
            }
        }
    }
}
