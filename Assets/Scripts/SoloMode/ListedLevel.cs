using ConvoSystem;
using Sound;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoloMode
{
    public class ListedLevel : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        /// <summary>
        /// The level this listed level object represents; level is played when this listed level item is selected by the player.
        /// </summary>
        private Level level;

        /// <summary>
        /// button component on this listed level.
        /// An OnClick is added to this via the LevelLister script when this object is created.
        /// </summary>
        [SerializeField] public Button button;

        /// <summary>
        /// Text displaying the level and clear status.
        /// </summary>
        /// <remarks>clear status may be moved to its own toggleable object in the future.</remarks>
        [SerializeField] public TextMeshProUGUI label;

        /// <summary>
        /// The "X" that displays if this level has been cleared or not
        /// </summary>
        [SerializeField] private GameObject clearIcon;

        /// <summary>
        /// should be set externally after this is instantiated by the LevelLister
        /// </summary>
        [HideInInspector] public LevelLister levelLister;
        [HideInInspector] public int levelIndex;

        public void SetLevel(Level level)
        {
            this.level = level;
            label.text = level.levelName;

            if (!level.RequirementsMet())
            {
                // button.interactable = false;
                label.color = Color.gray;
            }

            if (!level.IsCleared())
            {
                clearIcon.SetActive(false);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            label.text = level.levelName + " <";
            levelLister.scrollPositionTargetOffset = Utils.CalculateFocusedScrollPosition(levelLister.levelScrollRect, GetComponent<RectTransform>());
            levelLister.selectedLevelIndexes[levelLister.selectedTabIndex] = levelIndex;
            levelLister.CursorSound();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            label.text = level.levelName;
        }
    }
}
