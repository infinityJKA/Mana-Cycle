using Battle.Board;
using System.Collections.Generic;
using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

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
        }

        /// <summary>
        /// Unlock the passed achievement and save its unlock status in playerPrefs.
        /// After unlocked it will no longer be greyed out in the achievements list.
        /// </summary>
        /// <param name="achievement">The achievement to unlock</param>
        public void UnlockAchievement(Achievement achievement)
        {
            if (achievement.unlocked) return;
            achievement.UnlockPlayerPref();
            // NOTE: if too much going on, disable this notification if steam is initialized since they'll get the notificaiton there too
            achievementNotifyQueue.Enqueue(achievement);
        }

        /// <summary>
        /// Unlock an achievement from script via ID, for achievements not using the objective system.
        /// </summary>
        /// <param name="id">internal id of the achievement, ex. <c>Lv13Clear</c></param>
        public void UnlockAchievement(string id)
        {
            foreach (Achievement achievement in database.achievements)
            {
                if (achievement.id == id)
                {
                    UnlockAchievement(achievement);
                    return;
                }
            }
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

                // If objectives list is empty do not unlock it - it must be unlocked via another script
                if (achievement.requirements.Count == 0) continue;

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

            // don't run if steamworks disabled - game is either not run through steam or is webgl/standalone pc build
            #if !DISABLESTEAMWORKS
            UpdateSteamAchievements();
            #endif
        }

        #if !DISABLESTEAMWORKS
        public void UpdateSteamAchievements() {
            if (!SteamManager.Initialized) return;
            foreach (Achievement achievement in database.achievements)
            {
                if (achievement.unlocked) {
                    SteamUserStats.SetAchievement(achievement.id);
                }
            }
            SteamUserStats.StoreStats();
        }
        #endif
    }
}