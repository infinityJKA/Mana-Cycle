using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PostGameMenu : MonoBehaviour
{

    [SerializeField] private List<GameObject> MenuItems;
    private int currentSelection = 0;
    private double appearTime;
    public bool timerRunning {get; private set;}
    [SerializeField] private GameObject MenuUI;
    private TransitionScript transitionHandler;

    /** player 1 if solo mode, otherwise winning board in versus */
    private GameBoard board;

    // if this post game menu has been displayed
    private bool displayed = false;

    // Rematch button - text changed to "retry" in solo mode
    public TMPro.TextMeshProUGUI rematchTextGUI;

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

                displayed = true;
                timerRunning = false;

                if (Storage.gamemode == Storage.GameMode.Solo)
                {
                    int levelID = Storage.level.GetInstanceID();

                    // if not endless mode and is winner, level is cleared
                    bool clearedBefore = PlayerPrefs.GetInt(levelID+"_Cleared", 0) == 1;
                    bool cleared = board.isWinner() || (Storage.level.time == -1 && Storage.level.scoreGoal == 0);
                    if (cleared) PlayerPrefs.SetInt(levelID+"_Cleared", 1);

                    // set highscore if level was cleared
                    if (cleared) {
                        int highScore = PlayerPrefs.GetInt(levelID+"_HighScore", 0);
                        PlayerPrefs.SetInt(levelID+"_HighScore", Math.Max(board.hp, highScore));
                    }

                    setMenuSong();
                    Time.timeScale = 1f;
                    
                    // if first clear, immediately exit back to solomenu; otherwise, open menu
                    if (!clearedBefore && cleared) {
                        transitionHandler.WipeToScene("SoloMenu", i:true);
                    } else {
                        MenuUI.SetActive(true);
                        Time.timeScale = 0f;
                        // when in solo mode, disable css button
                        MenuItems[1].SetActive(false);
                        MenuItems.RemoveAt(1);

                        rematchTextGUI.text = "Retry";
                    }
                }
                else
                {
                    MenuUI.SetActive(true);
                    Time.timeScale = 0f;
                    // when in multi, disable solo button
                    MenuItems[2].SetActive(false);
                    MenuItems.RemoveAt(2);

                    rematchTextGUI.text = "Rematch";
                }

                EventSystem.current.SetSelectedGameObject(null);
                MoveCursor(0);
            }

            Storage.convoEndedThisInput = false;
        }
    }

    public void AppearWithDelay(double s, GameBoard board)
    {
        if (timerRunning || displayed) return;
        this.board = board;
        appearTime = s;
        timerRunning = true;
    }

    // alot of code is repeated from the pause menu script. could be cleaned up later
    public void MoveCursor(int amount)
    {
        currentSelection -= amount;
        currentSelection = Utils.mod(currentSelection, MenuItems.Count);
        EventSystem.current.SetSelectedGameObject(MenuItems[currentSelection]);
    }

    public void SelectOption()
    {
        if (Storage.convoEndedThisInput) return;
        // Debug.Log(MenuItems[currentSelection]);
        MenuItems[currentSelection].GetComponent<Button>().onClick.Invoke();
    }

    public void SelectRematchButton()
    {
        SceneManager.LoadScene("ManaCycle");
    }

    public void SelectBackToMainButton()
    {
        setMenuSong();
        Time.timeScale = 1f;
        transitionHandler.WipeToScene("3dMenu", i: true);
    }

    public void SelectBackToCSS()
    {
        setMenuSong();
        Time.timeScale = 1f;
        transitionHandler.WipeToScene("CharSelect", i: true);
    }

    public void SelectBackToSolo()
    {
        setMenuSong();
        Time.timeScale = 1f;
        transitionHandler.WipeToScene("SoloMenu", i: true);
    }

    public void setMenuSong(){
        SoundManager.Instance.musicSource.Stop();
        SoundManager.Instance.musicSource.clip = SoundManager.Instance.mainMenuMusic;
        SoundManager.Instance.musicSource.Play();
    }


}
