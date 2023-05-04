using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

using ConvoSystem;
using Battle;

namespace SoloMode {
    [CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Levels")]
    public class Level : ScriptableObject {
        [SerializeField] public string levelName = "Level";
        [SerializeField] public string description = "One of the levels of time";

        /** Conversation that happens before this level */
        [SerializeField] public Conversation conversation;

        /** Conversations that happen during the level, each with their own condition */
        [SerializeField] public List<MidLevelConversation> midLevelConversations;
        
        /** Amount of time to complete the level, in seconds. */
        [SerializeField] public int time = 300;
        /** Points needed to complete the level **/
        [SerializeField] public int scoreGoal = 2000;

        /** Total length of the cycle. */
        [SerializeField] public int cycleLength = 7;
        /** Amount of unique colors in the cycle. */
        [SerializeField] public int cycleUniqueColors = 5;

        /** Falling delay of pieces in this level. **/
        [SerializeField] public float fallTime = 0.8f;

        [SerializeField] public Battle.Battler battler;

        /** List of additional objectives that must be met to clear the stage **/
        [SerializeField] public List<Objective> objectives;

        /** The level that needs to be beaten before this level is unlocked. 
        could be expanded in the future to include things like high score requirements **/
        [SerializeField] public Level levelRequirement; 

        // Win on timer 0
        public bool survivalWin = false;

        // arcade mode / ai battle
        public bool aiBattle = false;

        public Battle.Battler opponent;

        // Battle music
        public AudioClip battleMusic;

        public bool RequirementsMet()
        {
            if (levelRequirement != null) return PlayerPrefs.GetInt(levelRequirement.levelName+"_Cleared", 0) == 1;
            else return true;
        }
    }

    #if (UNITY_EDITOR)
    [CustomEditor(typeof(Level))]
    public class LevelListerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            GUILayout.Label("\"Reset Level Progress\" will reset progress of ALL levels, resetting clear status and highscore.");

            if (GUILayout.Button("Reset Level Progress")) {
                PlayerPrefs.DeleteAll();
                Debug.Log("Level progress reset!");
            }
        }
    }
    #endif
}