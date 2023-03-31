using UnityEngine;
using System;
using UnityEditor;

[CreateAssetMenu(fileName = "Mid-Level Conversation", menuName = "ManaCycle/MidLevelConversation")]
public class MidLevelConversation : Conversation {
    /** All conditions that need to be met to show this convo */
    [SerializeField] public AppearConditionValue[] appearConditions;
    public enum AppearCondition {
        TimeRemaining,
        PointTotal,
        ManaClearedTotal,
        SpellcastTotal,
        TopCombo,
        BlobCount,
        /** 0=false, 1=true */
        Defeated,
        /** 0=false, 1=true */
        Won
    }

    [Serializable]
    public class AppearConditionValue {
        /** Condition where the dialogue will first be shown */
        public AppearCondition condition;
        /** Value, depends on the condition */
        public int value;

         /** Method to decide when the conversation should be shown, when all conditions are met */
        public bool ShouldAppear(GameBoard board)
        {
            switch (condition) {
                case AppearCondition.TimeRemaining: return board.timer.SecondsRemaining() <= value;
                case AppearCondition.PointTotal: return board.hp >= value;
                case AppearCondition.ManaClearedTotal: return board.GetTotalManaCleared() >= value;
                case AppearCondition.SpellcastTotal: return board.GetTotalSpellcasts() >= value;
                case AppearCondition.TopCombo: return board.GetHighestCombo() >= value;
                case AppearCondition.BlobCount: return board.GetBlobCount() >= value;
                case AppearCondition.Defeated: return value == 0 ? !board.isDefeated() : board.isDefeated();
                case AppearCondition.Won: return value == 0 ? !board.isWon() : board.isWon();
                default: return false;
            }
        }
    }

    public bool ShouldAppear(GameBoard board) {
        foreach (AppearConditionValue condition in appearConditions) {
            if (!condition.ShouldAppear(board)) return false;
        }
        return true;
    }
}

[CustomPropertyDrawer(typeof(MidLevelConversation.AppearConditionValue))]
public class AppearConditionValueDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        position.width *= 0.6f;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("condition"), GUIContent.none);
        position.x += position.width;
        position.width *= 0.667f;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);

        EditorGUI.EndProperty();
    }
}
