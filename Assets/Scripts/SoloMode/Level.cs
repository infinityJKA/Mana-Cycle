using System.Collections.Generic;
using UnityEngine;
using System;

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

        /** Modify delay before a piece is placed **/
        [SerializeField] public float slideTimeMulti = 1f;

        // the actual char you play as in the level, chosen by char select or automatically
        [System.NonSerialized] public Battle.Battler battler;

        // the battlers to choose from in charselect. if only one, they get auto picked like old system
        [SerializeField] public List<Battle.Battler> availableBattlers;

        /** List of additional objectives that must be met to clear the stage **/
        [SerializeField] public List<Objective> objectives;

        /** The level that needs to be beaten before this level is unlocked. 
        could be expanded in the future to include things like high score requirements **/
        [SerializeField] public Level levelRequirement; 

        // node structure of levels used in arcade mode to make a series of levels played back-to-back
        [SerializeField] public Level nextSeriesLevel = null;
        // automatically set by level
        [NonSerialized] public Level lastSeriesLevel;

        // Win on timer 0
        public bool survivalWin = false;

        // arcade mode / ai battle
        public bool aiBattle = false;

        public Battle.Battler opponent;

        // dictates AIController values. 1 is hardest, 0 is worst
        [Range(0,1)]
        public float aiDifficulty = 1;

        // Battle music
        public AudioClip battleMusic;

        public void Awake()
        {
            // set LastSeriesLevel
            if (nextSeriesLevel != null) nextSeriesLevel.lastSeriesLevel = this;
        }

        public bool RequirementsMet()
        {
            if (levelRequirement != null) return PlayerPrefs.GetInt(levelRequirement.levelName+"_Cleared", 0) == 1;
            else return true;
        }

        public bool GetCleared()
        {
            // get to last level in series, and return if it is cleared. an entire series must be cleared for it to show as cleared
            Level refLevel = this;
            while (refLevel.nextSeriesLevel != null) 
            {
                refLevel = refLevel.nextSeriesLevel;
            }

            return PlayerPrefs.GetInt(refLevel.levelName+"_Cleared", 0) == 1;
        }

        /// <summary> get amount of levels ahead of this one in the series </summary>
        public int GetAheadCount()
        {
            int count = 0;
            Level refLevel = this;
            while (refLevel.nextSeriesLevel != null) 
            {
                count++;
                refLevel = refLevel.nextSeriesLevel;
            }

            return count;
        }

        /// <summary> get sum of high score in level series</summary>
        public int GetHighScore()
        {
            int sum =  PlayerPrefs.GetInt(this.levelName+"_HighScore", 0);
            Level refLevel = this.nextSeriesLevel;
            
            while (refLevel != null)
            {
                sum += PlayerPrefs.GetInt(refLevel.levelName+"_HighScore", 0);
                refLevel = refLevel.nextSeriesLevel;
            }

            return sum;

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