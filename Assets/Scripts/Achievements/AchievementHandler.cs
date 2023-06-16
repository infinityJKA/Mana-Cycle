using Battle.Board;
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
        /// Notification object used to display achievements earned
        /// </summary>
        public AchievementNotification notification;

        /// <summary>
        /// If player receives multiple achievements at the same time, they will be queued to show up in this list.
        /// </summary>
        private Queue<Achievement> achievementNotifyQueue = new Queue<Achievement>();


        private void Update()
        {
            // If achievement animation is done and returned to default state, show the next achievement in the queue if there is one
            // do not advance queue while transition is happening
            if (achievementNotifyQueue.Count > 0 
                && !notification.animator.GetCurrentAnimatorStateInfo(0).IsName("AchievementNotificationAppear")
                && TransitionScript.transitionState == "none")
            {
                notification.ShowAchievement(achievementNotifyQueue.Dequeue());
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                UnlockAchievement(database.achievements[1]);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                UnlockAchievement(database.achievements[2]);
            }
        }

        /// <summary>
        /// Unlock the passed achievement and save its unlock status in playerPrefs.
        /// After unlocked it will no longer be greyed out in the achievements list.
        /// </summary>
        /// <param name="achievement">The achievement to unlock</param>
        public void UnlockAchievement(Achievement achievement)
        {
            if (achievement.unlocked) return;
            achievement.Unlock();
            Debug.Log("Unlocked " + achievement.id + ": " + achievement.displayName);

            achievementNotifyQueue.Enqueue(achievement);
        }

        /// <summary>
        /// To be run after a game is completed.
        /// Check all achievements in the database to see if it has been unlocked
        /// by accomplishments in the current battle. 
        /// </summary>
        /// <param name="board">The player's board</param>
        public void CheckAchievements(GameBoard board)
        {
            // 
            foreach (Achievement achievement in database.achievements)
            {
                // skip if already unlocked
                if (achievement.unlocked) continue;

                // check all objectives
                bool objectivesComplete = true;
                foreach (var objective in achievement.requirements)
                {
                    if (!objective.IsCompleted(board))
                    {
                        objectivesComplete = false;
                        break;
                    }
                }

                // if passed all objectives, earn the achievement
                if (objectivesComplete) UnlockAchievement(achievement);
            }
        }
    }
}