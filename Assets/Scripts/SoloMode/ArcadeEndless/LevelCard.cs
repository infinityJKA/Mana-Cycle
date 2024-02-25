using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

using Sound;

namespace SoloMode
{
    public class LevelCard : MonoBehaviour
    {
        // the level this card represents.
        [SerializeField] public Level level;
        // all the text gameobjects on the card, used for displaying level info
        [SerializeField] TextMeshProUGUI timeText;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] TextMeshProUGUI moneyRewardText;
        [SerializeField] GameObject itemRewardObject;
        [SerializeField] TextMeshProUGUI itemRewardText;

        [SerializeField] GameObject selectSFX, submitSFX;

        TransitionScript transitionHandler;

        void Start()
        {
            SetCardInfo();
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
        }

        public void SetCardInfo()
        {
            timeText.text = "Time: " + Utils.FormatTime(level.time);
            nameText.text = level.levelName;
            descriptionText.text = level.description;
            moneyRewardText.text = "+" + level.rewardAmount;

            if (level.itemReward != null) itemRewardText.text = "+" + level.itemReward.UseTypeToString() + " Item";
            else itemRewardObject.SetActive(false);

        }

        public void LoadLevel()
        {
            Instantiate(submitSFX);
            Storage.level = level;
            transitionHandler.WipeToScene("ManaCycle");
        }

        public void OnSelect()
        {
            Instantiate(selectSFX);
        }
        
    }
}