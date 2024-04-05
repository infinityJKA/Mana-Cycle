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
            SoundManager.Instance.StopBGM();
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
            if (Storage.level != null)
            {
                portrait.sprite = Storage.level.battler.sprite;

                AchievementHandler.Instance.UnlockAchievement("ArcadeWin" + Storage.level.battler.battlerId);

                if (Storage.lives == 3 && Storage.hp == 2000) {
                    AchievementHandler.Instance.UnlockAchievement("ArcadeWinNoDamage");
                }
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