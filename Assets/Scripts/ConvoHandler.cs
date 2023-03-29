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
    [SerializeField] private GameObject s1Portrait;
    [SerializeField] private GameObject s2Portrait;
    [SerializeField] private Image s1Img;
    [SerializeField] private Image s2Img;
    [SerializeField] private GameObject s1NameBox;
    [SerializeField] private GameObject s2NameBox;
    [SerializeField] private TMPro.TextMeshProUGUI s1NameText;
    [SerializeField] private TMPro.TextMeshProUGUI s2NameText;
    [SerializeField] private TMPro.TextMeshProUGUI dialougeText;

    /** current index of the conversation's dialogue lines */
    private int index;

    // Update is called once per frame
    void Update()
    {
        if (!convoUI.activeSelf) return;

        if (Input.GetKeyDown(inputScript.Cast))
        {
            index++;
            if (index >= convo.dialogueList.Length){
                EndConvo();
            }
            else{
                RefreshObjects();
            }
        }
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
        RefreshObjects();
    }

    void EndConvo()
    {
        // once the end of the convo is reached, transition to manacycle scene where the level will begin
        if (level != null)
        {
            GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("ManaCycle");
        }
        else
        {
            convoUI.SetActive(false);
            GameObject.Find("LevelLister").GetComponent<LevelLister>().SetFocus(true);
        }
        
    }

    void RefreshObjects()
    {
        var dialogue = convo.dialogueList[index];
        dialougeText.text = dialogue.text;

        if (dialogue.leftSpeaker != null)
        {
            s1Portrait.SetActive(true);
            s1NameBox.SetActive(true);
            s1Img.sprite = dialogue.leftSpeaker.sprite;
            s1NameText.text = dialogue.leftSpeaker.displayName;
        } else {
            // narrator
            s1Portrait.SetActive(false);
            s1NameBox.SetActive(false);
        }

        if (dialogue.rightSpeaker != null)
        {
            s2Portrait.SetActive(true);
            s2NameBox.SetActive(true);
            s2Img.sprite = dialogue.rightSpeaker.sprite;
            s2NameText.text = dialogue.rightSpeaker.displayName;
        } else {
            // narrator
            s2Portrait.SetActive(false);
            s2NameBox.SetActive(false);
        }
        
        

        if (dialogue.rightFocused)
        {
            s1Img.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            s2Img.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            s1Img.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            s2Img.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
        
    }

}
