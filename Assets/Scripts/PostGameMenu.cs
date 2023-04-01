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
            if (appearTime <= 0 && !board.IsCasting() && !board.convoPaused)
            {
                // Play convos if amy are remaining
                bool convoPlayed = board.CheckMidLevelConversations();
                if (convoPlayed) return;

                timerRunning = false;

                
                if (board.isWinner() && Storage.gamemode == Storage.GameMode.Solo)
                {
                    int levelID = Storage.level.GetInstanceID();
                    bool cleared = PlayerPrefs.GetInt(levelID+"_Cleared", 0) == 1;
                    if (!cleared)
                    {
                        PlayerPrefs.SetInt(levelID+"_Cleared", 1);

                        int highScore = PlayerPrefs.GetInt(levelID+"_HighScore", 0);
                        PlayerPrefs.SetInt(levelID+"_HighScore", Math.Max(board.hp, highScore));
                    }

                    Time.timeScale = 1f;
                    transitionHandler.WipeToScene("SoloMenu", i:true);
                }
                else
                {
                    MenuUI.SetActive(true);
                    Time.timeScale = 0f;
                    // when in solo mode, hide css button. when in multi, hide solo button.
                    if (Storage.gamemode == Storage.GameMode.Solo)
                    {
                        MenuItems[2].SetActive(false);
                        MenuItems.RemoveAt(2);
                    }
                    else
                    {
                        MenuItems[3].SetActive(false);
                        MenuItems.RemoveAt(3);
                    }
                    
                }

                EventSystem.current.SetSelectedGameObject(null);
                MoveCursor(0);
            }

            Storage.convoEndedThisInput = false;
        }
    }

    public void AppearWithDelay(double s, GameBoard winner)
    {
        board = winner;
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
        Time.timeScale = 1f;
        transitionHandler.WipeToScene("3dMenu", i: true);
    }

    public void SelectBackToCSS()
    {
        Time.timeScale = 1f;
        transitionHandler.WipeToScene("CharSelect", i: true);
    }

    public void SelectBackToSolo()
    {
        Time.timeScale = 1f;
        transitionHandler.WipeToScene("SoloMenu", i: true);
    }
}
