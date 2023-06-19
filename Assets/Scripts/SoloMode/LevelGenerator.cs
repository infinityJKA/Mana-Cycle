using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

using Random=UnityEngine.Random;

using Battle;
using ConvoSystem;

namespace SoloMode
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private List<Battle.Battler> usableBattlerList;
        [SerializeField] private List<Item> itemRewardPool;
        [SerializeField] private Conversation defaultConvo;

        public Level Generate(float difficulty = 0.5f, bool VersusLevelsEnabled = true, bool SoloLevelsEnabled = false, Battler battler = null, Level lastLevel = null)
        {
            Level newLevel = ScriptableObject.CreateInstance<Level>();

            // if both solo and versus levels can generate, randomly pick between the two by disabling one
            if (VersusLevelsEnabled && SoloLevelsEnabled)
            {
                VersusLevelsEnabled = (Random.value >= 0.5f);
                SoloLevelsEnabled = !VersusLevelsEnabled; 
            }

            newLevel.levelName = "Generated level";
            newLevel.description = "Default generated level description";
            newLevel.conversation = defaultConvo;
            newLevel.generateNextLevel = true;
            newLevel.lastSeriesLevel = lastLevel;
            newLevel.levelDifficulty = difficulty;
            // lowest difficutly = 8 minute timer, every .1 increase of difficulty will subtract 30 seconds. 
            newLevel.time = (8 * 60) - ((int) (difficulty*10))*30;
            // also randomly add from 0-2 minutes in 30sec increments
            newLevel.time += Random.Range(0,4) * 30;

            // if battler is given by method caller, set level battler. otherwise make battler random
            newLevel.battler = (battler != null) ? battler : usableBattlerList[(int) Random.Range(0, usableBattlerList.Count-1)];

            // 30% chance of item reward. TODO add item weights / rarities
            if (Random.value <= 0.3)
            {
                newLevel.itemReward = itemRewardPool[Random.Range(0,itemRewardPool.Count)];
            }

            // generate an ai battle level
            if (VersusLevelsEnabled)
            {
                // set opponent to random battler
                newLevel.aiBattle = true;
                newLevel.opponent = usableBattlerList[(int) Random.Range(0, usableBattlerList.Count-1)];
                newLevel.aiDifficulty = difficulty;
                newLevel.levelName = "Vs. " + newLevel.opponent.displayName;
                newLevel.description = "Fight a Level " + ((int) (newLevel.aiDifficulty*10f)) + " " + newLevel.opponent.displayName + "!";
                newLevel.CalculateRewardAmount();
            }
            // generate a solo level with objectives
            else if (SoloLevelsEnabled)
            {
                // not implemented yet
            }


            return newLevel;
        }
        
    }
}