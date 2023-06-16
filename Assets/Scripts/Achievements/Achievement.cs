using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// #if (UNITY_EDITOR)
// using UnityEditor;
// #endif

using SoloMode;

namespace Achievements {
    [CreateAssetMenu(fileName = "Achievement", menuName = "ManaCycle/Achievement")]
    public class Achievement : ScriptableObject
    {
        /// <summary>
        /// Name that is shown to the user in the achievments list.
        /// </summary>
        public string displayName;

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
    }

    // #if (UNITY_EDITOR)
    // [CustomPropertyDrawer(typeof(Achievement))]
    // public class AchievementDrawer : PropertyDrawer
    // {
    //     // Draw the property inside the given rect
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         // Using BeginProperty / EndProperty on the parent property means that
    //         // prefab override logic works on the entire property.
    //         EditorGUI.BeginProperty(position, label, property);

    //         // Draw label
    //         position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

    //         // Draw fields - pass GUIContent.none to each so they are drawn without labels
    //         position.width *= 0.6f;
    //         EditorGUI.PropertyField(position, property.FindPropertyRelative("condition"), GUIContent.none);
    //         position.x += position.width;
    //         position.width *= 0.6f;
    //         EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);
    //         position.x += position.width;
    //         position.width *= 0.6f;
    //         EditorGUI.PropertyField(position, property.FindPropertyRelative("inverted"), GUIContent.none);

    //         EditorGUI.EndProperty();
    //     }
    // }
    // #endif
}
