using System.Collections.Generic;
using UnityEngine;
using System;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

using ConvoSystem;
using Battle;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SoloMode {
    [CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Level")]
    public class Level : ScriptableObject {
        [SerializeField] public string levelId = "";
        [SerializeField] public string levelName = "Level";
        [SerializeField] public string description = "One of the levels of time";

        /** Conversation that happens before this level. if null, convo hasn't been loaded yet */
        public Conversation conversation {get; private set;}

        [SerializeField] private LocalizedAsset<Conversation> conversationEntry;

        private void OnEnable() {
            if (conversationEntry == null) {
                Debug.LogError(levelName+" has no cutscene table set");
                return;
            }

            Debug.Log("loading localized cutscene with key "+conversationEntry.TableEntryReference.Key);
            conversationEntry.LoadAssetAsync();
            conversationEntry.AssetChanged += UpdateConversationLocale;
        }

        private void OnDisable() {
            conversationEntry.AssetChanged -= UpdateConversationLocale;
        }

        // To be run when the name language string needs to be updated
        private void UpdateConversationLocale(Conversation localizedConvo) {
            if (localizedConvo == null) {
                Debug.LogError(levelName+" has no localized cutscene for current language");
            }

            conversation = localizedConvo;
        }

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
        [NonSerialized] public Battler battler;

        // the battlers to choose from in charselect. if only one, they get auto picked like old system
        [SerializeField] public List<Battler> availableBattlers;

        // set a fixed background to appear in this level. if null, choose randomly
        [SerializeField] public GameObject backgroundOverride = null;

        // Whether or not ability use is permitted in this level
        [SerializeField] public bool abilityEnabled = true;

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

        private void OnValidate() {
            if (levelId == "" || levelId == null) {
                levelId = levelName.Replace(" ", "");
            }
        }

        public bool requirementsMet
        {
            get {
                if (levelRequirement != null) return levelRequirement.isCleared;
                else return true;
            }
        }

        public bool isCleared {
            get {
                return ProgressData.current.levels.ContainsKey(levelId);
            }
            set {
                if (value == true && !ProgressData.current.levels.ContainsKey(levelId)) {
                    ProgressData.current.levels[levelId] = new LevelData();
                } else if (value == false) {
                    ProgressData.current.levels.Remove(levelId);
                }
            }
        }

        public static bool IsLevelCleared(string id) {
            return ProgressData.current.levels.ContainsKey(id);
        }

        public bool isSeriesCleared => finalLevel.isCleared;

        /// <summary> get amount of levels ahead of this one in the series </summary>
        public int aheadCount
        {
            get {
                int count = 0;
                Level refLevel = this;
                while (refLevel.nextSeriesLevel != null) 
                {
                    count++;
                    refLevel = refLevel.nextSeriesLevel;
                }

                return count;
            }
        }

        /// <summary> get amount of levels behind this one in the series </summary>
        public int behindCount
        {
            get {
                int count = 0;
                Level refLevel = this;
                while (refLevel.lastSeriesLevel != null) 
                {
                    count++;
                    refLevel = refLevel.lastSeriesLevel;
                }

                return count;
            }
        }

        /// <summary>
        /// get the length of the chain of levels that are required to beat before this one.
        /// Effectively the level number in the current tab minus one.
        /// </summary>
        /// <value></value>
        public int requirementCount
        {
            get {
                int count = 0;
                Level refLevel = this;
                while (refLevel.levelRequirement != null) 
                {
                    count++;
                    refLevel = refLevel.levelRequirement;
                }

                return count;
            }
        }

        /// <summary> return first level in series </summary>
        public Level rootLevel {
            get {
                Level refLevel = this;
                while (refLevel.lastSeriesLevel != null)
                {
                    refLevel = refLevel.lastSeriesLevel;
                }

                return refLevel;
            }
        }

        /// <summary> return last level in series </summary>
        public Level finalLevel {
            get {
                Level refLevel = this;
                while (refLevel.nextSeriesLevel != null) 
                {
                    refLevel = refLevel.nextSeriesLevel;
                }
                return refLevel;
            }
        }

        public int highScore {
            get {
                if(!ProgressData.current.levels.ContainsKey(levelId)) return 0;
                return ProgressData.current.levels[levelId].highScore;
            }
            set {
                ProgressData.current.levels[levelId].highScore = value;
            }
        }
        public int finalHighScore => finalLevel.highScore;

        /// <summary>
        /// fastest time in milliseconds
        /// </summary>
        public int fastestTime {
            get {
                if(!ProgressData.current.levels.ContainsKey(levelId)) return -1;
                return ProgressData.current.levels[levelId].fastestTime;
            }
            set {
                ProgressData.current.levels[levelId].fastestTime = value;
            }
        }

        public bool isEndless => time == -1 && scoreGoal == 0;

        public void CalculateRewardAmount()
        {
            // to be balanced
            // decrease amount if level has item reward set in generator
            rewardAmount = (int) (5 * Mathf.Floor((float) (aiDifficulty * 250 * (itemReward == null ? 1 : 0.6) + 50) / 5));
        }

        public string scoreLeaderboardKey => levelId+"_Score";
        public string timeLeaderboardKey => levelId+"_Time";

        public void BeginLevel() {
            Storage.level = this;
            Storage.lives = lives;

            // if multiple chars can be chosen from, go to char select
            if (availableBattlers.Count > 1)
            {
                GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("CharSelect");
            }
            // if only one available char, set battler and go to manacycle
            else 
            {
                battler = Storage.level.availableBattlers[0];
                GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("ManaCycle");
            }
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(Level))]
    public class LevelListerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            Level level = (Level) target;

            GUILayout.Label("PlayerPrefs progress:");
            GUILayout.Label("High score: "+level.highScore);

            GUILayout.Label(level.requirementsMet ? "Level unlocked" : "Unlock this level");

            if (GUILayout.Button("Unlock this level")) {
                level.levelRequirement.isCleared = true;
                Debug.Log("cleared progress of "+level.levelName);
            }
            
            GUILayout.Label("Reset the progress of this level");

            if (GUILayout.Button("Reset Level Progress")) {
                ProgressData.current.levels.Remove(level.levelId);
                ProgressData.Save();
                Debug.Log("cleared progress of "+level.levelId);
            }

            GUILayout.Label("Clear ALL save data (levels, achievements, platform-specific settings)");

            if (GUILayout.Button("Reset ALL Progress")) {
                ProgressData.ClearALLProgress();
                ProgressData.Save();
                Debug.Log("All progress reset!");
            }
        }
    }
    #endif
}