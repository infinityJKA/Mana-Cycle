using UnityEngine;
using Achievements;
using System.Collections.Generic;

namespace MainMenu {
    /// <summary>
    /// Controls input rebinding and rebinding window
    /// </summary>
    public class AchievementsMenu : MonoBehaviour
    {
        [SerializeField] private InputScript inputScript;
        [SerializeField] private GameObject settingsWindow;

        [SerializeField] public AchievementNotification achievementPrefab;

        [SerializeField] public AchievementHandler handler;

        [SerializeField] public Transform contentTransform;

        public void Update()
        {
            if (Input.GetKeyDown(inputScript.Pause))
            {
                HideMenu();
            }
        }

        public void ShowMenu()
        {
            settingsWindow.SetActive(false);
            gameObject.SetActive(true);
            LoadAchievementsTable();
        }

        public void HideMenu()
        {
            gameObject.SetActive(false);
            settingsWindow.SetActive(true);
        }

        public void LoadAchievementsTable()
        {
            foreach (Transform child in contentTransform) Destroy(child.gameObject);

            // sort achievements by unlocked first
            var achievementList = new List<Achievement>(handler.database.achievements);
            achievementList.Sort((ach1, ach2) => ach2.unlocked.CompareTo(ach1.unlocked));

            foreach (var achievement in achievementList)
            {
                var newAchv = Instantiate(achievementPrefab, contentTransform);
                newAchv.ShowAchievement(achievement);
            }
        }
    }
}
