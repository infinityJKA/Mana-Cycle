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

    /** Board that won the game, or player1 in solo mode */
    private GameBoard winningBoard;


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
            if (appearTime <= 0 && !winningBoard.IsCasting() && !winningBoard.convoPaused)
            {
                // Play convos if amy are remaining
                bool convoPlayed = winningBoard.CheckMidLevelConversations();
                if (convoPlayed) return;

                timerRunning = false;

                if (Storage.gamemode == Storage.GameMode.Solo && !Storage.level.cleared)
                {
                    // if solo mode, imediatly go back to solo menu
                    Storage.level.cleared = true;
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
        }


    }

    public void AppearWithDelay(double s, GameBoard winner)
    {
        winningBoard = winner;
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
