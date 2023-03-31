using UnityEngine;
using UnityEngine.EventSystems;

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

    /** Objects in the scene that should be disabled while convo is active */
    [SerializeField] private GameObject[] disableDuringConvo;

    /** Objects in the scene to permanently disable when a convo begins */
    [SerializeField] private GameObject[] disablePermanentOnConvo;

    /** Turns on and off the tutorial mask in the ManaCycle scene. **/
    [SerializeField] private TutorialDimMask tutorialDimMask;

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
        EventSystem.current.SetSelectedGameObject(null);

        foreach (GameObject obj in disableDuringConvo) {
            obj.SetActive(false);
        }
        foreach (GameObject obj in disablePermanentOnConvo) {
            obj.SetActive(false);
        }

        var midLevelConvo = convo as MidLevelConversation;
        if (midLevelConvo != null && tutorialDimMask != null) {
            tutorialDimMask.Show();
            tutorialDimMask.MaskTarget(midLevelConvo.tutorialMaskID);
        }

        DisplayConvoLine();
    }

    public void StartConvo(Conversation convo, GameBoard board) {
        SetBoard(board);
        StartConvo(convo);
    }

    void EndConvo()
    {
        if (board != null) {
            // Try to automatically start the next conversation avaialble. If none are, move on
            bool nextConvoPlayed = board.CheckMidLevelConversations();
            if (nextConvoPlayed) return;
        }

        Storage.convoEndedThisInput = true;

        foreach (GameObject obj in disableDuringConvo) {
            obj.SetActive(true);
        }

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

        if (board != null) {
            board.convoPaused = false;
            Time.timeScale = 1;
            board = null;
        }
    }

    // Is called once for each dialogue line
    void DisplayConvoLine()
    {
        var dialogue = convo.dialogueList[index];

        var text = dialogue.text;
        if (board != null) {
            text = text.Replace("{cycle0}", board.cycle.manaColorStrings[(int)board.cycle.GetColor(0)]);
            text = text.Replace("{cycle1}", board.cycle.manaColorStrings[(int)board.cycle.GetColor(1)]);
            text = text.Replace("{cycle2}", board.cycle.manaColorStrings[(int)board.cycle.GetColor(2)]);
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
