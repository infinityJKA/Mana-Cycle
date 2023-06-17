using UnityEngine;
using System;
using System.Linq;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

using Battle.Board;

namespace SoloMode {
    [Serializable]
    public class Objective {
        /** Condition where the dialogue will first be shown */
        public ObjectiveCondition condition;

        /** Value, depends on the condition */
        public int value;
        /** String value, can contain comma seperatesd lists, used by level name / battler name objectives **/
        public string stringValue = "Item1, Item2";
        /** Bool value, used by defeated/won objective **/
        public bool boolValue = true;

        // if not null, replaces the status message displayed on the objective list
        public string statusOverride = "";

        // lose if condition is met
        public bool inverted = false;

        /** Method to decide when the conversation should be shown, when all conditions are met */
        public bool IsCompleted(GameBoard board)
        {
            switch (condition) {
                case ObjectiveCondition.TimeRemaining: return board.timer.SecondsRemaining() <= value;
                case ObjectiveCondition.PointTotal: return board.hp >= value;
                case ObjectiveCondition.ManaClearedTotal: return board.GetTotalManaCleared() >= value;
                case ObjectiveCondition.SpellcastTotal: return board.GetTotalSpellcasts() >= value;
                case ObjectiveCondition.ManualSpellcastTotal: return board.GetManualSpellcasts() >= value;
                case ObjectiveCondition.TopCombo: return board.GetHighestCombo() >= value;
                case ObjectiveCondition.BlobCount: return board.GetBlobCount() >= value;
                case ObjectiveCondition.Survive: return !(board.timer.TimeUp() || board.IsWinner()) ^ boolValue;
                case ObjectiveCondition.Defeated: return !board.IsDefeated() ^ boolValue;
                case ObjectiveCondition.Won: return !board.WonAndNotCasting() ^ boolValue;
                case ObjectiveCondition.TopCascade: return board.GetHighestCascade() >= value;
                case ObjectiveCondition.LevelName: return board.level != null && stringValue.Split(", ").Contains(board.level.name);
                case ObjectiveCondition.BattlerName: return stringValue.Split(", ").Contains(board.Battler.displayName);
                case ObjectiveCondition.HighestSingleDamage: return board.highestSingleDamage >= value;
                case ObjectiveCondition.Lives: return board.lives >= value;
                default: return false;
            }
        }

        public string Status(GameBoard board) {
            if (statusOverride.Length > 0) return statusOverride;
            if (!inverted) {
                switch (condition) {
                    case ObjectiveCondition.PointTotal: return board.hp+"/"+value+" Points";
                    case ObjectiveCondition.ManaClearedTotal: return board.GetTotalManaCleared()+"/"+value+" Mana Cleared";
                    case ObjectiveCondition.SpellcastTotal: return board.GetTotalSpellcasts()+"/"+value+" Spellcasts";
                    case ObjectiveCondition.Survive: return "Survive!";
                    case ObjectiveCondition.TopCombo: return "Best Combo: " + board.GetHighestCombo()+"/"+value;
                    case ObjectiveCondition.TopCascade: return "Best Cascade: " + board.GetHighestCascade()+"/"+value;
                    default: return "This is an objective";
                }
            }
            else {
                switch (condition) {
                    case ObjectiveCondition.ManualSpellcastTotal: return "Spellcast only " + board.GetManualSpellcasts()+"/"+(value-1) + (value <= 2 ? " time" : "times");
                    default: return "evil objective gang";
                }
            }
        }
    } // close objective class

    public enum ObjectiveCondition {
        TimeRemaining,
        PointTotal,
        ManaClearedTotal,
        SpellcastTotal,
        ManualSpellcastTotal,
        TopCombo,
        TopCascade,
        BlobCount,

        Survive,
        Defeated,
        Won,

        LevelName, // level's name must match string value, used in achievements
        BattlerName, // battler name must match, used in achievements
        None, // used for progress stat var value if achievement should not track progress

        HighestSingleDamage,
        Lives
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

            var condition = property.FindPropertyRelative("condition");

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            position.width *= 0.5f;
            EditorGUI.PropertyField(position, condition, GUIContent.none);
            position.x += position.width + 2;
            position.width *= 0.7f;

            bool showInvert = true; // will be set to false if invert isn't needed to be shown for this objective condition type

            // use value type corresponding to the objective requirement - 
            // levelName uses string levelName property, all others will use the value int

            // string
            if (
                condition.enumValueIndex == (int)ObjectiveCondition.LevelName 
                || condition.enumValueIndex == (int)ObjectiveCondition.BattlerName
            ) { 
                EditorGUI.PropertyField(position, property.FindPropertyRelative("stringValue"), GUIContent.none);
            } 
            
            // bool
            else if (
                condition.enumValueIndex == (int)ObjectiveCondition.Survive
                || condition.enumValueIndex == (int)ObjectiveCondition.Defeated 
                || condition.enumValueIndex == (int)ObjectiveCondition.Won
            ) { 
                EditorGUI.PropertyField(position, property.FindPropertyRelative("boolValue"), GUIContent.none);
                showInvert = false;
            } 
            
            // int (default)
            else { 
                EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);
            }

            if (showInvert) {
                position.x += position.width + 4;
                position.width *= 0.7f;
                EditorGUI.PropertyField(position, property.FindPropertyRelative("inverted"), GUIContent.none);
            }

            // position.x += position.width + 4;
            // position.width *= 0.7f;
            // EditorGUI.PropertyField(position, property.FindPropertyRelative("statusOverride"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
    #endif
}