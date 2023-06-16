using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

using Sound;
using Achievements;

namespace SoloMode
{
    public class ArcadeWinScreen : MonoBehaviour
    {
        [SerializeField] Image portrait;
        private TransitionScript transitionHandler;
        [SerializeField] bool mobile;

        void Start()
        {
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
            if (Storage.level != null)
            {
                portrait.sprite = Storage.level.battler.sprite;

                AchievementHandler.Instance.UnlockAchievement("ArcadeWin_" + Storage.level.battler.name);
            }
        }

        void Update()
        {
            if (Input.anyKeyDown)
            {
                transitionHandler.WipeToScene("MainMenu", reverse:true);
                SoundManager.Instance.SetBGM(SoundManager.Instance.mainMenuMusic);
            }
        }
    }
}