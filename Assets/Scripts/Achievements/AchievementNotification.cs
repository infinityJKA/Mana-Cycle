using UnityEngine;
using UnityEngine.UI;

namespace Achievements
{
    public class AchievementNotification : MonoBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI titleLabel;

        [SerializeField] TMPro.TextMeshProUGUI descLabel; // only used in achievements menu

        [SerializeField] Image backgroundImage, iconImage;

        [SerializeField] Color unlockedBgColor, lockedBgColor, unlockedIconColor, lockedIconColor;

        /// <summary>
        /// This field will not be used if this is in the Achievements menu; only for notifications
        /// if in achievements menu this will be null, SHowAchievement will ignore animation if animator not present
        /// </summary>
        [SerializeField] private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }


        public void ShowAchievement(Achievement achievement)
        {
            titleLabel.text = achievement.displayName;
            iconImage.sprite = achievement.icon;

            // TODO: implement achievement locked/unlocked system.
            if (achievement.displayName != "Let Me Be Clear")
            {
                backgroundImage.color = unlockedBgColor;
                iconImage.color = unlockedIconColor;
            } else
            {
                backgroundImage.color = lockedBgColor;
                iconImage.color = lockedIconColor;
            }

            if (descLabel) descLabel.text = achievement.description;

            if (animator) animator.SetTrigger("Appear");
        }
    }
}