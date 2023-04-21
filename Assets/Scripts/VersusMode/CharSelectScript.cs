using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Battle;
using Sound;

namespace VersusMode {
    public class CharSelectScript : MonoBehaviour
    {
        [SerializeField] private InputScript[] inputScripts;
        [SerializeField] private List<Battler> battlerList;
        [SerializeField] private GameObject portraitDisp;
        [SerializeField] private GameObject nameDisp;
        [SerializeField] private GameObject typeToggle;
        [SerializeField] private GameObject typeLabel;
        [SerializeField] private AudioClip switchSFX;
        [SerializeField] private AudioClip selectSFX;
        private Battler currentChar;
        private TMPro.TextMeshProUGUI nameText;
        private TMPro.TextMeshProUGUI typeText;
        private Image portraitImg;
        private TransitionScript transitionHandler;

        private bool lockedIn = false;
        private bool charSelectorFocused = true;
        private int charSelection = 0;

        // Start is called before the first frame update
        void Start()
        {
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
            nameText = nameDisp.GetComponent<TMPro.TextMeshProUGUI>();
            portraitImg = portraitDisp.GetComponent<Image>();
            typeText = typeLabel.GetComponent<TMPro.TextMeshProUGUI>();
            UpdateLock();
        }

        // Update is called once per frame
        void Update()
        {
            foreach (InputScript inputScript in inputScripts) {
                if (Input.GetKeyDown(inputScript.Cast))
                {
                    if (charSelectorFocused)
                    {
                        lockedIn = !lockedIn;
                        if (lockedIn) SoundManager.Instance.PlaySound(selectSFX);
                        UpdateLock();
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
                        SoundManager.Instance.PlaySound(switchSFX);
                    }
                }

                if (Input.GetKeyDown(inputScript.Right) && !lockedIn)
                {
                    if (charSelectorFocused)
                    {
                        charSelection++;
                        SoundManager.Instance.PlaySound(switchSFX);
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

                if (Input.GetKeyDown(inputScript.Pause)){
                    transitionHandler.WipeToScene("3dMenu", i: true);
                }
            }

            // keep selections in bounds
            charSelection = Utils.mod(charSelection, battlerList.Count);


            // update objects
            currentChar = battlerList[charSelection];
            
            // show arrows if not locked in
            if (lockedIn) {
                nameText.text = currentChar.displayName;
            } else {
                nameText.text = "< "+currentChar.displayName+" >";
            }

            portraitImg.sprite = currentChar.sprite;

            if (!charSelectorFocused){
                EventSystem.current.SetSelectedGameObject(typeToggle);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(nameDisp);
            }
        }

        void UpdateLock(){
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
}
