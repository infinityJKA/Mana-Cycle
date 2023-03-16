using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CharSelectScript : MonoBehaviour
{
    [SerializeField] private InputScript inputScript;
    [SerializeField] private List<Battler> battlerList;
    [SerializeField] private GameObject portraitDisp;
    [SerializeField] private GameObject nameDisp;
    private TMPro.TextMeshProUGUI nameText;
    private Image portraitImg;

    private bool lockedIn = false;
    private int charSelection = 0;

    // Start is called before the first frame update
    void Start()
    {
        nameText = nameDisp.GetComponent<TMPro.TextMeshProUGUI>();
        portraitImg = portraitDisp.GetComponent<Image>();
        updateLock();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(inputScript.Cast))
        {
            lockedIn = !lockedIn;
            updateLock();
        }

        if (Input.GetKeyDown(inputScript.Left) && !lockedIn)
        {
            charSelection--;
        }

        if (Input.GetKeyDown(inputScript.Right) && !lockedIn)
        {
            charSelection++;
        }

        // keep selection in bounds
        charSelection = Utils.mod(charSelection, battlerList.Count);

        // update objects
        Battler currentChar = battlerList[charSelection];
        nameText.text = currentChar.displayName;
        portraitImg.sprite = currentChar.sprite;
    }

    void updateLock(){
        if (!lockedIn){
            // unlocked
            portraitImg.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            nameText.fontStyle = (TMPro.FontStyles) FontStyle.Normal;
        }
        else{
            // locked in
            portraitImg.color = new Color(1.0f, 1.0f, 1.0f, 1f);
            nameText.fontStyle = (TMPro.FontStyles) FontStyle.Bold;

            // timerManager.Refresh();
        }
    }

    public bool GetLocked(){
        return lockedIn;
    }
}
