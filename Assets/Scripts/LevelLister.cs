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
    [SerializeField] private GameObject listObject;
    private TMPro.TextMeshProUGUI listText;
    private RectTransform listTransform;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;

    [SerializeField] private GameObject highScoreBG;
    [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

    /** Inputs that control the level list */
    [SerializeField] private InputScript[] inputScripts;
    /** y offset when scrolling (?) */
    [SerializeField] private float yOffset;
    [SerializeField] private float scrollAmount;
    /** Index of current level selected */
    private int selection;
    /** Descent between lines in the level list */
    // private float decLine;

    /** current targeted scroll position */
    private Vector2 targetPosition;
    /** current scroll velocity */
    private Vector2 vel = Vector2.zero;

    /** If the level list is currently focused, instead of dialogue */
    private bool focused; 

    [SerializeField] private AudioClip moveSFX;
    [SerializeField] private AudioClip selectSFX;
    [SerializeField] private AudioClip errorSFX;

    [SerializeField] GameObject upArrow;

    [SerializeField] GameObject downArrow;

    // Start is called before the first frame update
    void Start()
    {
        focused = true;
        listText = listObject.GetComponent<TMPro.TextMeshProUGUI>();
        // decLine = (listText.font.faceInfo.descentLine);
        listTransform = listObject.GetComponent<RectTransform>();

        selection = GetNextLevel();

        RefreshList();
    }

    // Update is called once per frame
    void Update()
    {
        if (!focused) return;

        foreach (InputScript inputScript in inputScripts) {

            if (Input.GetKeyDown(inputScript.Up))
            {
                selection--;
                RefreshList();
                // commented out because these cause errors now????? will look at later
                // SoundManager.Instance.PlaySound(moveSFX, pitch : 1.1f);
            }

            if (Input.GetKeyDown(inputScript.Down))
            {
                selection++;
                RefreshList();
                // SoundManager.Instance.PlaySound(moveSFX);
            }

            // pause - go back to main menu
            if (Input.GetKeyDown(inputScript.Pause))
            {
                GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("3dMenu", i : true);
            }

            // cast - open selected level
            if (Input.GetKeyDown(inputScript.Cast))
            {
                if (!levelsList[selection].RequirementsMet()) 
                {
                    SoundManager.Instance.PlaySound(errorSFX);
                    return;
                }
                
                Storage.level = levelsList[selection]; 
                Storage.gamemode = Storage.GameMode.Solo;
                convoHandler.StartLevel(levelsList[selection]);
                focused = false;
                Storage.levelSelectedThisInput = true;
                SoundManager.Instance.PlaySound(selectSFX);
            }
        }

        // smoothly update displayed y position of level list
        listTransform.position = Vector2.SmoothDamp(listTransform.position, targetPosition, ref vel, 0.1f);
    }

    void RefreshList()
    {
        selection = Math.Clamp(selection, 0, levelsList.Length-1);
        
        string newText = "";
        // add and subtract for extra lines at the start and end of list
        for (int i = -30; i < levelsList.Length + 30; i++)
        {
            // flavor lines
            if (i < 0 || i >= levelsList.Length){
                newText += "################" + "\n";
            }
            else{
                Level level = levelsList[i];

                bool cleared = PlayerPrefs.GetInt(level.GetInstanceID()+"_Cleared", 0) == 1;

                if (i == selection && level.RequirementsMet()) newText += " <color=#FFFFFF>";

                else if (!level.RequirementsMet()) newText += "<color=#015706>";

                else newText += "<color=#00ff10>";

                newText += level.levelName;

                if (i == selection) newText += " <";
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
        Level selectedLevel = levelsList[selection];
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
        targetPosition = new Vector2(listTransform.position.x, selection*(scrollAmount)*Screen.height + yOffset*Screen.height);
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