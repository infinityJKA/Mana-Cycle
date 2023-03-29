using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelLister : MonoBehaviour
{
    [SerializeField] private Level[] levelsList;
    [SerializeField] private GameObject listObject;
    private TMPro.TextMeshProUGUI listText;
    private RectTransform listTransform;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] private InputScript[] inputScripts;
    [SerializeField] private float yOffset;
    private int selection = 0;
    private float decLine;

    private Vector2 targetPosition;
    private Vector2 vel = Vector2.zero;

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
            }

            if (Input.GetKeyDown(inputScript.Down))
            {
                selection++;
            }

            if (Input.GetKeyDown(inputScript.Pause))
            {
                GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("3dMenu", i : true);
            }

            if (Input.GetKeyDown(inputScript.Cast))
            {
                // THIS IS TEMP
                GameObject.Find("ConvoHandler").GetComponent<ConvoHandler>().StartConvo(0);
                focused = false;
            }

            selection = Math.Clamp(selection, 0, levelsList.Length-1);

        }

        RefreshList();
        listTransform.position = Vector2.SmoothDamp(listTransform.position, targetPosition, ref vel, 0.1f);
    }

    void RefreshList()
    {
        // update text
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

                newText += l.levelName;
                if (i == selection)
                {
                    newText += " <";
                }
                newText += "\n";
            }

        }

        listText.text = newText;
        descriptionText.text = levelsList[selection].description;

        // update position
        targetPosition = new Vector2(listTransform.position.x, selection*(50+decLine) + yOffset);
    }

}
