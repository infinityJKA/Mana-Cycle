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
                portrait.material = Storage.level.battler.material;

                //Check for achievements
                // AchievementHandler AH = FindObjectOfType<AchievementHandler>();

                string battlerID = Storage.level.battler.battlerId;
                Debug.Log("BATTLERID = "+ battlerID);
                if(battlerID == "Infinity"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinInfinity");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Aqua"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinAqua");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Pyro"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinPyro");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Psychic"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinPsychic");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Geo"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinGeo");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Trainbot"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinTrainbot");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "zman"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinzman");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Electro"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinElectro");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Romra"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinRomra");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Bithecary"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinBithecary");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "BetterYou"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinBetterYou");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Erif"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinErif");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Xuirbo"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinXuirbo");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}
                else if (battlerID == "Mirrored"){AchievementHandler.Instance.UnlockAchievement("ArcadeWinMirrored");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");}


                if (Storage.lives == 3 && Storage.hp == 2000) {
                    AchievementHandler.Instance.UnlockAchievement("ArcadeWinNoDamage");Debug.Log("ACHIVEMENT SHOULD BE WON HERE");
                }

                AchievementHandler.Instance.UpdateSteamAchievements();

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