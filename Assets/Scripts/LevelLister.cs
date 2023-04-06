using UnityEngine;
using System;

public class LevelLister : MonoBehaviour
{
    // morgan please comment your code in the future :(

    /** Conversation handler, to run conversation when level is selected */
    [SerializeField] private ConvoHandler convoHandler;

    /** List of levels to select */
    [SerializeField] private Level[] levelsList;

    // List level object caches
    /** list text component where the lines are written out */
    [SerializeField] private TMPro.TextMeshProUGUI listText;

    // self explanatory
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;

    [SerializeField] private GameObject highScoreBG;
    [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

    /** Inputs that control the level list */
    [SerializeField] private InputScript[] inputScripts;

    /** Initial offset of the list, saved on start from anchored position */
    [SerializeField] private Vector2 listOffset;
    /** amount to scroll by for each line. set to font size on start */
    [SerializeField] private float scrollAmount;

    /** Index of current level selected */
    private int selectedLevelIndex;
    /** Descent between lines in the level list */
    // private float decLine;

    /** current targeted scroll position */
    private Vector2 targetPosition;
    /** current scroll velocity */
    private Vector2 vel = Vector2.zero;

    /** If the level list is currently focused, instead of dialogue */
    private bool focused; 

    // menu sfx
    [SerializeField] private AudioClip moveSFX;
    [SerializeField] private AudioClip selectSFX;
    [SerializeField] private AudioClip errorSFX;

    // currently unused references
    [SerializeField] GameObject upArrow;
    [SerializeField] GameObject downArrow;

    // Start is called before the first frame update
    void Start()
    {
        focused = true;
        // decLine = (listText.font.faceInfo.descentLine);

        // last level selected when in this window.
        if (Storage.lastLevelSelectedIndex == -1) Storage.lastLevelSelectedIndex = GetNextLevel();
        else selectedLevelIndex = Storage.lastLevelSelectedIndex;
        Storage.lastLevelSelectedIndex = selectedLevelIndex;

        listOffset = listText.rectTransform.anchoredPosition;
        scrollAmount = listText.fontSize;

        RefreshList();
    }

    // Update is called once per frame
    void Update()
    {
        if (!focused) return;

        foreach (InputScript inputScript in inputScripts) {

            if (Input.GetKeyDown(inputScript.Up))
            {
                selectedLevelIndex--;
                Storage.lastLevelSelectedIndex = selectedLevelIndex;
                RefreshList();
                SoundManager.Instance.PlaySound(moveSFX, pitch : 1.1f);
            }

            if (Input.GetKeyDown(inputScript.Down))
            {
                selectedLevelIndex++;
                Storage.lastLevelSelectedIndex = selectedLevelIndex;
                RefreshList();
                SoundManager.Instance.PlaySound(moveSFX);
            }

            // pause - go back to main menu
            if (Input.GetKeyDown(inputScript.Pause))
            {
                GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("3dMenu", i : true);
            }

            // cast - open selected level
            if (Input.GetKeyDown(inputScript.Cast))
            {
                if (!levelsList[selectedLevelIndex].RequirementsMet()) 
                {
                    SoundManager.Instance.PlaySound(errorSFX);
                    return;
                }
                
                Storage.level = levelsList[selectedLevelIndex]; 
                Storage.gamemode = Storage.GameMode.Solo;
                convoHandler.StartLevel(levelsList[selectedLevelIndex]);
                focused = false;
                Storage.levelSelectedThisInput = true;
                SoundManager.Instance.PlaySound(selectSFX);
            }
        }

        // smoothly update displayed y position of level list
        listText.rectTransform.anchoredPosition = 
        Vector2.SmoothDamp(listText.rectTransform.anchoredPosition, targetPosition, ref vel, 0.1f);
    }


    private static int flavorLineCount = 30;
    void RefreshList()
    {
        selectedLevelIndex = Math.Clamp(selectedLevelIndex, 0, levelsList.Length-1);
        
        string newText = "";
        // add and subtract for extra lines at the start and end of list
        for (int i = -flavorLineCount; i < levelsList.Length + flavorLineCount; i++)
        {
            // flavor lines
            if (i < 0 || i >= levelsList.Length){
                newText += "################" + "\n";
            }
            else{
                Level level = levelsList[i];

                bool cleared = PlayerPrefs.GetInt(level.GetInstanceID()+"_Cleared", 0) == 1;

                if (i == selectedLevelIndex && level.RequirementsMet()) newText += " <color=#FFFFFF>";

                else if (!level.RequirementsMet()) newText += "<color=#015706>";

                else newText += "<color=#00ff10>";

                newText += level.levelName;

                if (i == selectedLevelIndex) newText += " <";
                newText += " </color>";

                // clear check
                if (cleared)
                {
                    newText += "<color=#00ffdf> X</color>";
                }
                else
                {
                    newText += "<color=#000000> X</color>";
                }

                newText += "\n";
            }

        }
        listText.text = newText;

        // display the description and time of the selected level
        Level selectedLevel = levelsList[selectedLevelIndex];
        descriptionText.text = selectedLevel.description;

        bool selectedCleared = PlayerPrefs.GetInt(selectedLevel.GetInstanceID()+"_Cleared", 0) == 1;
        highScoreBG.SetActive(selectedCleared);
        highScoreText.text = "High Score: "+PlayerPrefs.GetInt(selectedLevel.GetInstanceID()+"_HighScore", 0);

        if (selectedLevel.time != -1)
        {
            timeText.text = "Time: " + Utils.FormatTime(selectedLevel.time);
        }
        else
        {
            timeText.text = "Time: ∞";
        }
        

        // update the targeted scroll position
        targetPosition = listOffset + Vector2.up * (flavorLineCount+selectedLevelIndex) * scrollAmount;
        // targetPosition = new Vector2(listTransform.position.x, selectedLevelIndex*(scrollAmount)*Screen.height + yOffset*Screen.height);
    }

    public void SetFocus(bool f){
        focused = f;
    }

    // loop through each level and return the index of the first one that hasn't been cleard.
    public int GetNextLevel()
    {
        for (int i = 0; i < levelsList.Length; i++)
        {
            if (!(PlayerPrefs.GetInt(levelsList[i].GetInstanceID()+"_Cleared", 0) == 1)) return i;
        }
        // if all levels are cleared, start at 0
        return 0;
    }
}