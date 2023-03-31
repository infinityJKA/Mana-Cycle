using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConvoHandler : MonoBehaviour
{
    /** The level in which the current conversation is being played in.
    This level's game challenge will be ran after the convo is over. */
    private Level level;

    /** Current conversation being played, should be the only convo in the level, 
    unless things change in the future */
    private Conversation convo;

    /** inputs for controlling the conversation */
    [SerializeField] private InputScript inputScript;

    /** object containing the conversation UI */
    [SerializeField] private GameObject convoUI;

    // references for objects within the convo UI 
    // (fyi Morgan: inactive objects can still be referenced, and even call functions on them, they just aren't updated with Update(). Gameobject.Find is slow)
    // [SerializeField] private GameObject s1Portrait;
    // [SerializeField] private GameObject s2Portrait;
    // [SerializeField] private Image s1Img;
    // [SerializeField] private Image s2Img;
    // [SerializeField] private GameObject s1NameBox;
    // [SerializeField] private GameObject s2NameBox;
    // [SerializeField] private TMPro.TextMeshProUGUI s1NameText;
    // [SerializeField] private TMPro.TextMeshProUGUI s2NameText;
    [SerializeField] private ConvoSpeaker leftSpeaker;

    [SerializeField] private ConvoSpeaker rightSpeaker;

    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;

    // Set board in case the conversation needs to read/write from it
    [SerializeField] private GameBoard board;

    /** current index of the conversation's dialogue lines */
    private int index;

    // Update is called once per frame
    void Update()
    {
        if (!convoUI.activeSelf) return;

        if (Input.GetKeyDown(inputScript.Cast) && !Storage.levelSelectedThisInput)
        {
            index++;
            if (index >= convo.dialogueList.Length){
                EndConvo();
            }
            else{
                DisplayConvoLine();
            }

        }

        Storage.levelSelectedThisInput = false;
    }

    public void StartLevel(Level level) {
        this.level = level;
        StartConvo(level.conversation);
    }

    public void StartConvo(Conversation convo)
    {
        this.convo = convo;
        index = 0;
        convoUI.SetActive(true);
        DisplayConvoLine();
    }

    public void StartConvo(Conversation convo, GameBoard board) {
        SetBoard(board);
        StartConvo(convo);
    }

    void EndConvo()
    {
        // once the end of the convo is reached, transition to manacycle scene where the level will begin
        if (level != null)
        {
            Storage.level = level;
            GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("ManaCycle");
        }
        else
        {
            convoUI.SetActive(false);

            // look for level list it and select if it exists, it won't if in manaCycle scene
            var levelLister = GameObject.Find("LevelLister");
            if (levelLister != null) {
                levelLister.GetComponent<LevelLister>().SetFocus(true);
            }
        }
        level = null;
        Time.timeScale = 1;

        if (board != null) {
            board.dialoguePaused = false;
            board = null;
        }
    }

    // Is called once for each dialogue line
    void DisplayConvoLine()
    {
        var dialogue = convo.dialogueList[index];

        var text = dialogue.text;
        if (board != null) {
            text = text.Replace("{cycle0}", board.cycle.manaColorStrings[0]);
            text = text.Replace("{cycle1}", board.cycle.manaColorStrings[1]);
            text = text.Replace("{cycle2}", board.cycle.manaColorStrings[2]);
            text = text.Replace("{spellcast}", board.inputScript.Cast.ToString());
        }
        dialogueText.text = text;

        leftSpeaker.SetSpeaker(dialogue.leftSpeaker, !dialogue.rightFocused);
        rightSpeaker.SetSpeaker(dialogue.rightSpeaker, dialogue.rightFocused);        
    }

    public void SetBoard(GameBoard board)
    {
        this.board = board;
    }
}
