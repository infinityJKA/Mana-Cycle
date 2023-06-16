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

        public void ShowNotification(int index)
        {
            Achievement achievement = database.achievements[index];
            notification.ShowAchievement(achievement);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                ShowNotification(1);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                ShowNotification(2);
            }
        }
    }
}