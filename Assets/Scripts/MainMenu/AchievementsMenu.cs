using UnityEngine;
using Achievements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace MainMenu {
    /// <summary>
    /// Controls input rebinding and rebinding window
    /// </summary>
    public class AchievementsMenu : MonoBehaviour
    {
        [SerializeField] private PlayerInput input;
        [SerializeField] private SettingsMenu settingsWindow;

        [SerializeField] public AchievementNotification achievementPrefab;

        [SerializeField] public AchievementHandler handler;

        [SerializeField] public Transform contentTransform;

        [SerializeField] public ScrollRect scrollRect;

        [SerializeField] public InputActionReference scrollInput;

        [SerializeField] public float scrollSpeed = 1f;

        private Vector2 scrollDir;

        public void Update()
        {
            if (input.actions["Cancel"].WasPressedThisFrame())
            {
                HideMenu();
                return;
            }

            scrollRect.normalizedPosition += scrollDir * scrollSpeed * Time.deltaTime;

            // scroll with navigation up/down
            scrollInput.action.performed += ctx =>
            {
                scrollDir = ctx.ReadValue<Vector2>();
            };
        }

        public void ShowMenu()
        {
            settingsWindow.gameObject.SetActive(false);
            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(scrollRect.gameObject);
            LoadAchievementsTable();
        }

        public void HideMenu()
        {
            gameObject.SetActive(false);
            settingsWindow.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(settingsWindow.achievementsButton);
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
