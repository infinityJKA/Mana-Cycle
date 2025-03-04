using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Random=UnityEngine.Random;
using TMPro;
using UnityEngine.Localization.Settings;

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
        [SerializeField] TextMeshProUGUI enemyStatText;
        [SerializeField] TextMeshProUGUI enemyHpText;

        [SerializeField] GameObject selectSFX, submitSFX;

        [SerializeField] Color[] possibleAccentColors;
        [SerializeField] Image[] accentColoredObjects;

        TransitionScript transitionHandler;

        void Start()
        {
            SetCardInfo();
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();

            Color c = possibleAccentColors[Random.Range(0, possibleAccentColors.Length - 1)];
            foreach(Image i in accentColoredObjects)
            {
                i.color = new Color(c.r, c.g, c.b, i.color.a);
            }
        }

        public void SetCardInfo()
        {
            if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                timeText.text = "時間制限: " + Utils.FormatTime(level.time);
                enemyStatText.text = String.Format("ダメージ: {0}x \n重力: {1}x \nスペシャル: {2}x \n", 
                level.enemyStats[ArcadeStats.Stat.DamageMult],
                level.enemyStats[ArcadeStats.Stat.QuickDropSpeed],
                level.enemyStats[ArcadeStats.Stat.SpecialGainMult]);
            }
            else{
                timeText.text = "Time: " + Utils.FormatTime(level.time);
                enemyStatText.text = String.Format("Damage: {0}x \nFall Speed: {1}x \nSpecial Gain: {2}x \n", 
                level.enemyStats[ArcadeStats.Stat.DamageMult],
                level.enemyStats[ArcadeStats.Stat.QuickDropSpeed],
                level.enemyStats[ArcadeStats.Stat.SpecialGainMult]);
            }


            
            nameText.text = level.levelName;
            descriptionText.text = level.description;
            moneyRewardText.text = "+" + level.rewardAmount;

            enemyHpText.text = level.enemyHp + " HP";

            if (level.itemReward != null) itemRewardText.text = "+" + level.itemReward.UseTypeToString();
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