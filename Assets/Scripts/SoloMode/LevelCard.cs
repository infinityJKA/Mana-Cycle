using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

// using Sound;

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

        void Start()
        {
            setCardInfo();
        }

        public void setCardInfo()
        {
            timeText.text = "Time: " + Utils.FormatTime(level.time);
            nameText.text = level.levelName;
            descriptionText.text = level.description;
        }
        
    }
}