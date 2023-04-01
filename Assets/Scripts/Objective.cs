using UnityEngine;
using System;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

[Serializable]
public class Objective {
    /** Condition where the dialogue will first be shown */
    public ObjectiveCondition condition;
    /** Value, depends on the condition */
    public int value;

        /** Method to decide when the conversation should be shown, when all conditions are met */
    public bool IsCompleted(GameBoard board)
    {
        switch (condition) {
            case ObjectiveCondition.TimeRemaining: return board.timer.SecondsRemaining() <= value;
            case ObjectiveCondition.PointTotal: return board.hp >= value;
            case ObjectiveCondition.ManaClearedTotal: return board.GetTotalManaCleared() >= value;
            case ObjectiveCondition.SpellcastTotal: return board.GetTotalSpellcasts() >= value;
            case ObjectiveCondition.TopCombo: return board.GetHighestCombo() >= value;
            case ObjectiveCondition.BlobCount: return board.GetBlobCount() >= value;
            case ObjectiveCondition.Survive: return board.timer.TimeUp() || board.isWinner();
            case ObjectiveCondition.Defeated: return value == 0 ? !board.isDefeated() : board.isDefeated();
            case ObjectiveCondition.Won: return value == 0 ? !board.wonAndNotCasting() : board.wonAndNotCasting();
            default: return false;
        }
    }
    public string Status(GameBoard board) {
        switch (condition) {
            case ObjectiveCondition.PointTotal: return board.hp+"/"+value+" Points";
            case ObjectiveCondition.ManaClearedTotal: return board.GetTotalManaCleared()+"/"+value+" Mana Cleared";
            case ObjectiveCondition.SpellcastTotal: return board.GetTotalSpellcasts()+"/"+value+" Spellcasts";
            case ObjectiveCondition.Survive: return "Survive!";
            default: return "This is an objective";
        }
    }
}

public enum ObjectiveCondition {
    TimeRemaining,
    PointTotal,
    ManaClearedTotal,
    SpellcastTotal,
    TopCombo,
    BlobCount,
    Survive,
    /** 0=false, 1=true */
    Defeated,
    /** 0=false, 1=true */
    Won,
}

#if (UNITY_EDITOR)
[CustomPropertyDrawer(typeof(Objective))]
public class ObjectiveDrawer : PropertyDrawer
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
#endif