using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Achievements 
{
    public class AchievementHandler : MonoBehaviour 
    {
        public static AchievementHandler Instance;
        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else{
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Database of achievements that can be earned throughout the game
        /// </summary>
        public AchievementDatabase database;

        /// <summary>
        /// If player receives multiple achievements at the same time, they will be queued to show up in this list.
        /// </summary>
        public List<Achievement> achievementNotifyQueue;

        /// <summary>
        /// Notification object used to display achievements earned
        /// </summary>
        public AchievementNotification notification;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                UnlockAchievement(database.achievements[1]);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                UnlockAchievement(database.achievements[2]);
            }
        }

        public void UnlockAchievement(Achievement achievement)
        {
            if (achievement.unlocked) return;
            achievement.Unlock();
            Debug.Log("Unlocked " + achievement.id + ": " + achievement.displayName);
            notification.ShowAchievement(achievement);
        }
    }
}