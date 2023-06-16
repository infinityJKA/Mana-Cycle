using UnityEngine;
using Achievements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainMenu {
    /// <summary>
    /// Controls input rebinding and rebinding window
    /// </summary>
    public class AchievementsMenu : MonoBehaviour
    {
        [SerializeField] private List<InputScript> inputScripts;
        [SerializeField] private GameObject settingsWindow;

        [SerializeField] public AchievementNotification achievementPrefab;

        [SerializeField] public AchievementHandler handler;

        [SerializeField] public Transform contentTransform;

        [SerializeField] public ScrollRect scrollRect;

        [SerializeField] public float scrollSpeed = 1f;

        public void Update()
        {
            foreach (InputScript inputScript in inputScripts)
            {
                if (Input.GetKeyDown(inputScript.Pause))
                {
                    HideMenu();
                    return;
                }

                if (Input.GetKey(inputScript.Down))
                {
                    scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
                }

                if (Input.GetKey(inputScript.Up))
                {
                    scrollRect.verticalNormalizedPosition += scrollSpeed * Time.deltaTime;
                }
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

            // List unlocked achievements first and then locked
            foreach (var achievement in handler.database.achievements)
            {
                if (!achievement.unlocked) continue;
                var newAchv = Instantiate(achievementPrefab, contentTransform);
                newAchv.ShowAchievement(achievement);
            }

            foreach (var achievement in handler.database.achievements)
            {
                if (achievement.unlocked) continue;
                var newAchv = Instantiate(achievementPrefab, contentTransform);
                newAchv.ShowAchievement(achievement);
            }
        }
    }
}
