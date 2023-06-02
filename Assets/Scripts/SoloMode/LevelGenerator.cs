using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

using Random=UnityEngine.Random;

namespace SoloMode
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private List<Battle.Battler> usableBattlerList;

        public Level Generate(float difficulty = 0.5f, bool VersusLevelsEnabled = true, bool SoloLevelsEnabled = false)
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
            // lowest difficutly = 8 minute timer, every .1 increase of difficulty will subtract 30 seconds. 
            newLevel.time = (8 * 60) - ((int) (difficulty*10))*30;
            // also randomly add from 0-2 minutes in 30sec increments
            newLevel.time += Random.Range(0,4) * 30;

            // generate an ai battle level
            if (VersusLevelsEnabled)
            {
                // set opponent to random battler
                newLevel.opponent = usableBattlerList[(int) Random.Range(0, usableBattlerList.Count-1)];
                newLevel.aiDifficulty = difficulty;
                newLevel.levelName = "Vs. " + newLevel.opponent.displayName;
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