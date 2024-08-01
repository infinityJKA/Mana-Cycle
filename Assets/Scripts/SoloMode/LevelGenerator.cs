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
        [SerializeField] private List<Battler> usableBattlerList;
        [SerializeField] private List<AudioClip> usableSongList;

        public Level Generate(float difficulty = 0.5f, bool VersusLevelsEnabled = true, bool SoloLevelsEnabled = false, Battler battler = null, Level lastLevel = null)
        {
            Level newLevel = ScriptableObject.CreateInstance<Level>();

            // if both solo and versus levels can generate, randomly pick between the two by disabling one
            if (VersusLevelsEnabled && SoloLevelsEnabled)
            {
                VersusLevelsEnabled = Random.value >= 0.5f;
                SoloLevelsEnabled = !VersusLevelsEnabled; 
            }

            newLevel.levelName = "Generated level";
            newLevel.levelId = "Generated";
            newLevel.description = "Default generated level description";
            newLevel.generateNextLevel = true;
            newLevel.lastSeriesLevel = lastLevel;
            newLevel.levelDifficulty = difficulty;
            // lowest difficutly = 8 minute timer, every .1 increase of difficulty will subtract 30 seconds. 
            newLevel.time = (8 * 60) - ((int) Mathf.Floor(difficulty*5))*30;
            // also randomly add from 0-2 minutes in 30sec increments
            newLevel.time += Random.Range(0,4) * 30;

            // if battler is given by method caller, set level battler. otherwise make battler random
            newLevel.battler = (battler != null) ? battler : usableBattlerList[(int) Random.Range(0, usableBattlerList.Count-1)];

            newLevel.battleMusic = usableSongList[Random.Range(0, usableSongList.Count-1)];

            // generate an ai battle level
            if (VersusLevelsEnabled)
            {
                // set opponent to random battler
                newLevel.aiBattle = true;
                newLevel.opponent = usableBattlerList[(int) Random.Range(0, usableBattlerList.Count-1)];

                float statDifficulty = 0f;
                // set ai difficulty and stats
                if (difficulty >= 0.2f) statDifficulty = difficulty * (Random.Range(4, 8) / 10f);
                newLevel.aiDifficulty = difficulty;

                // list of all stats effected by statDifficulty
                List<ArcadeStats.Stat> statsToChange = new List<ArcadeStats.Stat>
                {
                    ArcadeStats.Stat.DamageMult,
                    ArcadeStats.Stat.SpecialGainMult,
                    ArcadeStats.Stat.QuickDropSpeed
                };
                Utils.Shuffle(statsToChange);

                // Debug.Log(statsToChange.Count - 2);

                // spread statDifficulty across all stats to allocate
                for (int i = 0; i <= statsToChange.Count - 2; i++)
                {
                    ArcadeStats.Stat stat = statsToChange[i];

                    float toAllocate = statDifficulty * (Random.Range(0, 10) / 10f);
                    // Debug.Log("To allocate: " + toAllocate);

                    statDifficulty -= toAllocate;
                    if (statDifficulty <= 0) break;
                    // Debug.Log("statDifficulty:" + statDifficulty);
                    float deltaStat;
                    deltaStat = AllocateToStat(stat, toAllocate);

                    newLevel.enemyStats[stat] += deltaStat;
                }

                newLevel.enemyStats[statsToChange[statsToChange.Count - 1]] += AllocateToStat(statsToChange[statsToChange.Count - 1], statDifficulty);

                newLevel.enemyHp = (int) Mathf.Min(250 * Mathf.Ceil(difficulty * 5), 10000);
                // newLevel.enemyStats[ArcadeStats.Stat.DamageMult] = 999f;
                newLevel.levelName = "Vs. " + newLevel.opponent.displayName;
                newLevel.description = "Fight a Level " + ((int) (newLevel.aiDifficulty*10f)) + " " + newLevel.opponent.displayName + "!";
                // 1 in 4 chance for item reward
                if (Random.Range(0,3) == 0 && ArcadeStats.itemRewardPool != null) newLevel.itemReward = ArcadeStats.itemRewardPool[Random.Range(0, ArcadeStats.itemRewardPool.Count-1)];
                newLevel.CalculateRewardAmount();
                // Debug.Log("Card difficulty: " + difficulty);
            }
            // generate a solo level with objectives
            else if (SoloLevelsEnabled)
            {
                // not implemented yet
            }


            return newLevel;
        }

        private float AllocateToStat(ArcadeStats.Stat stat, float toAllocate)
        {
            float deltaStat = 0f;

            // using a switch case because different stats should scale differently.
            // TODO use serialized curves for each stat instead of a switch case?
            switch(stat)
            {
                case ArcadeStats.Stat.DamageMult:
                    deltaStat = (float) Math.Floor(20 * toAllocate) / 40f;
                    break;
                case ArcadeStats.Stat.SpecialGainMult:
                    deltaStat = (float) Math.Floor(20 * toAllocate) / 20f;
                    break;
                case ArcadeStats.Stat.QuickDropSpeed:
                    deltaStat = (float) Math.Min(Math.Floor(10 * toAllocate) / 40f * -1f, 0.125/2f);
                    break;
            }

            return deltaStat;
        }

        
    }
}