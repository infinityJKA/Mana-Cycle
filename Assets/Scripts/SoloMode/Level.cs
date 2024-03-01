using System.Collections.Generic;
using UnityEngine;
using System;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

using ConvoSystem;
using Battle;
using Random=UnityEngine.Random;

namespace SoloMode {
    [CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Level")]
    public class Level : ScriptableObject {
        [SerializeField] public string levelName = "Level";
        [SerializeField] public string description = "One of the levels of time";

        /** Conversation that happens before this level */
        [SerializeField] public Conversation conversation;

        /** Conversations that happen during the level, each with their own condition */
        [SerializeField] public List<MidLevelConversation> midLevelConversations;
        
        /** Amount of time to complete the level, in seconds. -1 for infinite time */
        [SerializeField] public int time = 300;
        /** Points needed to complete the level **/
        [SerializeField] public int scoreGoal = 2000;

        /// <summary>Amount of lives the player gets.<summary>
        [SerializeField] public int lives = 1;

        /** Total length of the cycle. */
        [SerializeField] public int cycleLength = 7;
        /** Amount of unique colors in the cycle. */
        [SerializeField] public int cycleUniqueColors = 5;

        [SerializeField] public bool lockPieceColors = true;
 
        /** Falling delay of pieces in this level. **/
        [SerializeField] public float fallTime = 0.8f;

        /** Modify delay before a piece is placed **/
        [SerializeField] public float slideTimeMult = 1f;

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

        // whether or not clearing this level will generate levels and take you to the arcade endless selection screen
        public bool generateNextLevel = false;
        // reward amount to give in arcade endless
        [NonSerialized] public int rewardAmount = 0;
        [NonSerialized] public Item itemReward = null;

        // enemy stats for ae
        [NonSerialized] public Dictionary<ArcadeStats.Stat, float> enemyStats = new Dictionary<ArcadeStats.Stat, float>(ArcadeStats.defaultStats);
        [NonSerialized] public int enemyHp = 2000;

        // dictates AIController values. 1 is hardest, 0 is worst
        [Range(0,1)]
        public float aiDifficulty = 1;

        // level difficulty used by generated levels
        public float levelDifficulty = 0;

        // the rate at which trash pieces are sent to the board. -1 (or any value less than 0) disables
        public float trashSendRate = -1;

        // Battle music
        public AudioClip battleMusic;

        public void Awake()
        {
            // this doesn't run consistiently for some reason, moved to gameboard
            // set LastSeriesLevel
            // Debug.Log("there i am 0,0");
            // if (nextSeriesLevel != null) nextSeriesLevel.lastSeriesLevel = this;
        }

        public bool RequirementsMet()
        {
            if (levelRequirement != null) return PlayerPrefs.GetInt(levelRequirement.levelName+"_Cleared", 0) == 1;
            else return true;
        }

        public bool IsCleared()
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

        /// <summary> get amount of levels behind this one in the series </summary>
        public int GetBehindCount()
        {
            int count = 0;
            Level refLevel = this;
            while (refLevel.lastSeriesLevel != null) 
            {
                count++;
                refLevel = refLevel.lastSeriesLevel;
            }

            return count;
        }

        /// <summary> return first level in series </summary>
        public Level GetRootLevel()
        {
            Level refLevel = this;
            while (refLevel.lastSeriesLevel != null)
            {
                refLevel = refLevel.lastSeriesLevel;
            }

            return refLevel;
        }

        /// <summary>Get the highscore of this level, or of the last level if in a series</summary>
        public int GetHighScore()
        {
            int score =  PlayerPrefs.GetInt(this.levelName+"_HighScore", 0);
            Level refLevel = this.nextSeriesLevel;
            
            while (refLevel != null)
            {
                score = PlayerPrefs.GetInt(refLevel.levelName+"_HighScore", 0);
                refLevel = refLevel.nextSeriesLevel;
            }

            return score;
        }

        public bool IsEndless() {
            return time == -1 && scoreGoal == 0;
        }

        public void CalculateRewardAmount()
        {
            // to be balanced
            // decrease amount if level has item reward set in generator
            rewardAmount = (int) ((aiDifficulty / time * 150000) * (itemReward == null ? 1 : 0.5));
        }
    }

    #if (UNITY_EDITOR)
    [CustomEditor(typeof(Level))]
    public class LevelListerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            Level level = (Level) target;

            GUILayout.Label("PlayerPrefs progress:");
            GUILayout.Label("High score: "+PlayerPrefs.GetInt(level.levelName+"_HighScore", 0));

            GUILayout.Label(level.RequirementsMet() ? "Level unlocked" : "Unlock this level");

            if (GUILayout.Button("Unlock this level")) {
                PlayerPrefs.SetInt(level.levelRequirement.levelName+"_Cleared", 1);
                Debug.Log("cleared progress of "+level.levelName);
            }
            
            GUILayout.Label("Reset the progress of this level");

            if (GUILayout.Button("Reset Level Progress")) {
                PlayerPrefs.DeleteKey(level.levelName+"_Cleared");
                PlayerPrefs.DeleteKey(level.levelName+"_HighScore");
                Debug.Log("cleared progress of "+level.levelName);
            }

            GUILayout.Label("Reset ALL player preferences and level status");

            if (GUILayout.Button("Reset ALL Progress")) {
                PlayerPrefs.DeleteAll();
                Debug.Log("All progress reset!");
            }
        }
    }
    #endif
}