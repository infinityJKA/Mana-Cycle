using System.Collections.Generic;
using System.IO;
using LootLocker.Requests;
using UnityEngine;

[System.Serializable]
public class ProgressData {
    public static ProgressData current {get; private set;}

    // ===== Saving & loading
    public const string fileName = "progressdata.json";
    public static readonly string filePath;
    static ProgressData() {
        filePath = Path.Join(Application.persistentDataPath, fileName);
        Load();
    }
    public static void Save() {
        FileStorageManager.Save(current, filePath, encrypt: true);

        // if logged in, save on backend
        if (LootLockerSDKManager.CheckInitialized()) {
            RemoteFileManager.UploadFile(filePath, fileName, isPublic: false);
        }
    }
    public static void Load() {
        current = FileStorageManager.Load<ProgressData>(filePath, decrypt: true);
    }

    public static void ClearALLProgress() {
        current = new ProgressData();
    }

    // ===== DATA

    /// <summary>
    /// This dictionary will only contain levels that have been cleared at least once.
    /// Key = levelId, value = contains high score, fastest time, possibly other data later on.
    /// If a levelId is not present in this dictionary's keys then the level has not been cleared yet
    /// </summary>
    public Dictionary<string, LevelData> levels = new Dictionary<string, LevelData>();

    /// <summary>
    /// List of all achievement IDs the player has unlocked
    /// </summary>
    public List<string> achievements = new List<string>();

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        ProgressData other = (ProgressData)obj;
        
        foreach (var kvp in levels) {
            string levelId = kvp.Key;
            if (other.levels.TryGetValue(levelId, out LevelData otherLevelData)) {
                LevelData levelData = levels[levelId];
                // if highscore or fastest time are not equal, progress data unequal
                if (levelData.highScore != otherLevelData.highScore || levelData.fastestTime != otherLevelData.fastestTime) {
                    Debug.LogWarning("Desync found, stopping equal check: "+levelId);
                    return false;
                }
            } else {
                // this has a level cleared that other does not; not equal
                return false;
            }
        }

        // Check but the other way around: other reference, current check
        foreach (var kvp in other.levels) {
            string levelId = kvp.Key;
            if (levels.TryGetValue(levelId, out LevelData levelData)) {
                LevelData otherLevelData = other.levels[levelId];
                // if highscore or fastest time are not equal, progress data unequal
                if (levelData.highScore != otherLevelData.highScore || levelData.fastestTime != otherLevelData.fastestTime) {
                    Debug.LogWarning("Desync found, stopping equal check: "+levelId);
                    return false;
                }
            } else {
                // this has a level cleared that other does not; not equal
                return false;
            }
        }

        // check achievement list equality
        if (achievements.Count != other.achievements.Count) return false;
        for (int i = 0; i < achievements.Count; i++) {
            if (achievements[i] != other.achievements[i]) {
                Debug.LogWarning("Desync found, stopping equal check: "+achievements[i]);
                return false;
            }
        }

        Debug.Log("Progressdatas are identical.");
        return true;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17; // Choose prime numbers for initial values
            hash = hash * 19 + levels.GetHashCode(); // Combine hash codes
            hash = hash * 23 + achievements.GetHashCode();
            return hash;
        }
    }
}

[System.Serializable]
public class LevelData {
    public int highScore;
    public int fastestTime; // clear time in milliseconds; used for leaderboards
}

[System.Serializable]
public struct MatchStats {
    /// <summary>Total amount of points earned (solo) or damage dealt (versus).
    public int totalScore;
    /// <summary>Total amount of mana this board has cleared</summary>
    public int totalManaCleared;
    /// <summary>Total amount of spellcasts this player has performed */</summary>
    public int totalSpellcasts;
    /// <summary> Highest combo performed by the player </summary>
    public int highestCombo;
    /// <summary>Highest cascade performed by the player </summary>
    public int highestCascade;
    /// <summary>Total amount of spellcast chains this player has started (aka. pressing spellcast and getting a chain of at least 1)</summary>
    public int totalManualSpellcasts;
    /// <summary>Highest single damage dealt in one spellcast during the battle </summary>
    public int highestSingleDamage;

    // ==== Versus only
    /// <summary>Total amount of damage countered</summary>
    public int totalDamageCountered;
}