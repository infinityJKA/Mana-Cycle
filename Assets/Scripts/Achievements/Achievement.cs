using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// #if (UNITY_EDITOR)
// using UnityEditor;
// #endif

using SoloMode;
using UnityEngine.SocialPlatforms.Impl;
using System.Globalization;

namespace Achievements {
    [System.Serializable]
    public class Achievement
    {
        /// <summary>
        /// Name that is shown to the user in the achievments list.
        /// </summary>
        public string displayName;

        /// <summary>
        /// Internal ID used to track the progress of this achievement
        /// </summary>
        public string id;

        /// <summary>
        /// Single sentence or longer description describing this achievement's requirements.
        /// </summary>
        public string description;

        /// <summary>
        /// Icon shown for this achievement.
        /// </summary>
        public Sprite icon;

        /// <summary>
        /// The stat that this achievement should display the progress of
        /// </summary>
        public ObjectiveCondition progressStat = ObjectiveCondition.None;

        /// <summary>
        /// All requirements that must pass as true to earn this achievement
        /// </summary>
        public List<Objective> requirements;

        /// <summary>
        /// Returns true if this achievement is shown as unlcoked from the PlayerPrefs.
        /// </summary>
        public bool unlocked 
        { 
            get 
            { 
                return PlayerPrefs.GetInt("ACH_" + id, 0) == 1;
            }
        }

        public void Unlock()
        {
            PlayerPrefs.SetInt("ACH_" + id, 1);
        }
    }
}