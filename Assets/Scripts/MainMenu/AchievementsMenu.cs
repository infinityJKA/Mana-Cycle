using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.EventSystems;
using Achievements;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

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

        public void Start()
        {
            GenerateAchivementList();
        }

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
        }

        public void HideMenu()
        {
            gameObject.SetActive(false);
            settingsWindow.SetActive(true);
        }

        public void GenerateAchivementList()
        {
            foreach (Transform child in contentTransform) Destroy(child.gameObject);

            foreach (var achievement in handler.database.achievements)
            {
                var newAchv = Instantiate(achievementPrefab, contentTransform);
                newAchv.ShowAchievement(achievement);
            }
        }
    }
}
