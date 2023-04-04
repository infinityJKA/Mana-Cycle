using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Random=UnityEngine.Random;

public class ConvoHandler : MonoBehaviour
{
    /** The level in which the current conversation is being played in.
    This level's game challenge will be ran after the convo is over. */
    private Level level;

    /** Current conversation being played, should be the only convo in the level, 
    unless things change in the future */
    private Conversation convo;

    /** inputs for controlling the conversation */
    [SerializeField] private InputScript[] inputScripts;

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

    [SerializeField] private TMPro.TextMeshProUGUI textLabel;

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

    /** If the current cutscene is a mid-level conversation. */
    private bool isMidLevelConvo;

    // [SerializeField] private AudioClip typeSound;

    // Update is called once per frame
    void Update()
    {
        if (!convoUI.activeSelf) return;

        foreach (InputScript inputScript in inputScripts) {

            if (Input.GetKeyDown(inputScript.Cast) && !Storage.levelSelectedThisInput)
            {
            // if typing, set typing to false which will cause the coroutine to finish the current line on next update
            if (typing) {
                typing = false;
            }

            // otherwise, display next line & finish any animation that may be happening
            else {
                index++;
                leftSpeaker.animating = false;
                rightSpeaker.animating = false;
                // end convo when past the last index; otherwise, next line
                if (index >= convo.dialogueList.Length){
                    EndConvo();
                } else {
                    DisplayConvoLine();
                }
                
            }
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
            isMidLevelConvo = true;
            tutorialDimMask.Show();
            tutorialDimMask.MaskTarget(midLevelConvo.tutorialMaskID);
        }

        DisplayConvoLine();
    }

    public void StartConvo(Conversation convo, GameBoard board) {
        this.board = board;
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

        var midLevelConvo = convo as MidLevelConversation;
        if (midLevelConvo != null && tutorialDimMask != null) {
            tutorialDimMask.Hide();
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

    /** Current line being shown */
    private ConversationLine line;
    /** Current text being typed, after formats */
    private string formattedText;
    // Is called once for each dialogue line
    void DisplayConvoLine()
    {
        line = convo.dialogueList[index];

        formattedText = line.text;
        if (board != null) {
            formattedText = formattedText.Replace("{cycle0}", board.cycle.manaColorStrings[(int)board.cycle.GetColor(0)]);
            formattedText = formattedText.Replace("{cycle1}", board.cycle.manaColorStrings[(int)board.cycle.GetColor(1)]);
            formattedText = formattedText.Replace("{cycle2}", board.cycle.manaColorStrings[(int)board.cycle.GetColor(2)]);
            formattedText = formattedText.Replace("{spellcast}", board.inputScript.Cast.ToString());
        }

        leftSpeaker.SetSpeaker(line.leftSpeaker, !line.rightFocused);
        rightSpeaker.SetSpeaker(line.rightSpeaker, line.rightFocused);

        leftSpeaker.StartAnim(line.leftAnim);
        rightSpeaker.StartAnim(line.rightAnim);

        // no type effect if midlevelconvo
        if (isMidLevelConvo) {
            textLabel.text = formattedText;
        } else {
            StartCoroutine(TypeText());
        }
    }

    [Tooltip("Typing speed, characters per second")]
    [SerializeField] float typeSpeed = 40;
    private bool typing = false;
    IEnumerator TypeText()
    {
        float t = 0;
        int prevCharIndex;
        int charIndex = 0;
        typing = true;

        // While typing hasn't been set to false and still text to write: show substring of typed chars
        while (typing && charIndex < formattedText.Length)
        {
            prevCharIndex = Mathf.Clamp(Mathf.FloorToInt(t), 0, formattedText.Length);

            t += Time.unscaledDeltaTime*typeSpeed;
            charIndex = Mathf.Clamp(Mathf.FloorToInt(t), 0, formattedText.Length);


            AudioClip speakerSFX = null;
            // If there is no speaker, don't use a speaker sound. otherwise, use speaker sfx from battler
            if (line.rightFocused && line.rightSpeaker != null) speakerSFX = line.rightSpeaker.voiceSFX;
            else if (line.leftSpeaker != null) speakerSFX = line.leftSpeaker.voiceSFX;

            // if char index and previous char index are different, we just drew a new char. play type sound
            // only play sound every other char because damn thats a lot of sounds
            if(speakerSFX != null && charIndex != prevCharIndex && charIndex%2 == 0) SoundManager.Instance.PlaySound(speakerSFX, pitch : Random.Range(1.4f,1.6f));

            textLabel.text = formattedText.Substring(0, charIndex);
            
            yield return null;
        }

        typing = false;
        textLabel.text = formattedText;
    }
}
