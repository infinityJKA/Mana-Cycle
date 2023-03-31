using System.Collections;
using System.Collections.Generic;
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

    /** Inputs that control the level list */
    [SerializeField] private InputScript[] inputScripts;
    /** y offset when scrolling (?) */
    [SerializeField] private float yOffset;
    /** Index of current level selected */
    private int selection = 0;
    /** Descent between lines in the level list */
    private float decLine;

    /** current targeted scroll position */
    private Vector2 targetPosition;
    /** current scroll velocity */
    private Vector2 vel = Vector2.zero;

    /** If the level list is currently focused, instead of dialogue */
    private bool focused;

    // Start is called before the first frame update
    void Start()
    {
        focused = true;
        listText = listObject.GetComponent<TMPro.TextMeshProUGUI>();
        decLine = (listText.font.faceInfo.descentLine);
        listTransform = listObject.GetComponent<RectTransform>();

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
            }

            if (Input.GetKeyDown(inputScript.Down))
            {
                selection++;
                RefreshList();
            }

            // pause - go back to main menu
            if (Input.GetKeyDown(inputScript.Pause))
            {
                GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("3dMenu", i : true);
            }

            // cast - open selected level
            if (Input.GetKeyDown(inputScript.Cast))
            {
                Storage.level = levelsList[selection];
                Storage.gamemode = Storage.GameMode.Solo;
                convoHandler.StartLevel(levelsList[selection]);
                focused = false;
            }
        }

        // smoothly update displayed y position of level list
        listTransform.position = Vector2.SmoothDamp(listTransform.position, targetPosition, ref vel, 0.1f);
    }

    void RefreshList()
    {
        selection = Math.Clamp(selection, 0, levelsList.Length-1);
        
        string newText = "";
        // add and subtract 20 for extra lines at the start and end of list
        for (int i = -20; i < levelsList.Length + 20; i++)
        {
            if (i < 0 || i >= levelsList.Length){
                // flavor lines
                newText += "########" + "\n";
            }
            else{
                Level l = levelsList[i];

                if (i == selection)
                {
                    newText += " <color=#FFFFFF>";
                }
                newText += l.levelName;
                if (i == selection)
                {
                    newText += " <</color>";
                }
                newText += "\n";
            }

        }
        listText.text = newText;

        // display the description and time of the selected level
        descriptionText.text = levelsList[selection].description;
        timeText.text = "Time: " + Utils.FormatTime(levelsList[selection].time);

        // update the targeted scroll position
        targetPosition = new Vector2(listTransform.position.x, selection*(50+decLine) + yOffset);
    }

    public void SetFocus(bool f){
        focused = f;
    }
}
