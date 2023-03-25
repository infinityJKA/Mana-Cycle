using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharSelectScript : MonoBehaviour
{
    [SerializeField] private InputScript inputScript;
    [SerializeField] private List<Battler> battlerList;
    [SerializeField] private GameObject portraitDisp;
    [SerializeField] private GameObject nameDisp;
    [SerializeField] private GameObject typeToggle;
    [SerializeField] private GameObject typeLabel;
    private Battler currentChar;
    private TMPro.TextMeshProUGUI nameText;
    private TMPro.TextMeshProUGUI typeText;
    private Image portraitImg;

    private bool lockedIn = false;
    private bool charSelectorFocused = true;
    private int charSelection = 0;

    // Start is called before the first frame update
    void Start()
    {
        nameText = nameDisp.GetComponent<TMPro.TextMeshProUGUI>();
        portraitImg = portraitDisp.GetComponent<Image>();
        typeText = typeLabel.GetComponent<TMPro.TextMeshProUGUI>();
        updateLock();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(inputScript.Cast))
        {
            if (charSelectorFocused)
            {
                lockedIn = !lockedIn;
                updateLock();
            }
            else
            {
                typeToggle.GetComponent<Toggle>().isOn = !typeToggle.GetComponent<Toggle>().isOn;
            }
        }

        if (Input.GetKeyDown(inputScript.Left) && !lockedIn)
        {
            if (charSelectorFocused)
            {
                charSelection--;
            }
        }

        if (Input.GetKeyDown(inputScript.Right) && !lockedIn)
        {
            if (charSelectorFocused)
            {
                charSelection++;
            }
        }

        if (Input.GetKeyDown(inputScript.Up) && !lockedIn)
        {
            charSelectorFocused = true;
        }

        if (Input.GetKeyDown(inputScript.Down) && !lockedIn)
        {
            charSelectorFocused = false;
        }

        // keep selections in bounds
        charSelection = Utils.mod(charSelection, battlerList.Count);


        // update objects
        currentChar = battlerList[charSelection];
        nameText.text = currentChar.displayName;
        portraitImg.sprite = currentChar.sprite;

        if (!charSelectorFocused){
            EventSystem.current.SetSelectedGameObject(typeToggle);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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

    public Battler GetChoice(){
        return currentChar;
    }

    public bool GetPlayerType(){
        return typeToggle.GetComponent<Toggle>().isOn;
    }

    public void UpdateTypeDisp(){
        if (typeToggle.GetComponent<Toggle>().isOn)
        {
            typeText.text = "Player";
        }
        else
        {
            typeText.text = "CPU";
        }
    }
}
