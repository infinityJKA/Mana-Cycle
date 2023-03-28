using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelLister : MonoBehaviour
{
    [SerializeField] private Level[] levelsList;
    [SerializeField] private TMPro.TextMeshProUGUI listText;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    private int selection = 0;
    [SerializeField] private InputScript[] inputScripts;
    // Start is called before the first frame update
    void Start()
    {
        RefreshList();
    }

    // Update is called once per frame
    void Update()
    {
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

            selection = Math.Clamp(selection, 0, levelsList.Length-1);

        }

        RefreshList();
    }

    void RefreshList()
    {
        string newText = "";
        for (int i = 0; i < levelsList.Length; i++)
        {
            Level l = levelsList[i];

            newText += l.levelName;
            if (i == selection)
            {
                newText += " <";
            }
            newText += "\n";
        }
        listText.text = newText;
        descriptionText.text = levelsList[selection].description;
    }
}
