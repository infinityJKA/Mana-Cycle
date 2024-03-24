using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData {
    public static SaveData current {get; private set;}

    // ===== Saving & loading
    private static readonly string filePath;
    static SaveData() {
        filePath = Path.Join(Application.persistentDataPath, "savedata.sav");
        Load();
    }
    public static void Save() {
        FileStorageManager.Save(current, filePath, encrypt: true);
    }
    public static void Load() {
        current = FileStorageManager.Load<SaveData>(filePath, decrypt: true);
    }
    public static void RemoveAllSaveData() {
        current = new SaveData();
    }

    // ===== DATA

    /// <summary>
    /// This dictionary will only contain levels that have been cleared at least once.
    /// Key = levelId, value = contains high score, fastest time, possibly other data later on.
    /// If a levelId is not present in this dictionary's keys then the level has not been cleared yet
    /// </summary>
    public Dictionary<string, LevelData> levels = new Dictionary<string, LevelData>();

    /// <summary>
    /// Hashset of all achievements the player has unlocked
    /// </summary>
    public HashSet<string> achievements = new HashSet<string>();
}

[System.Serializable]
public class LevelData {
    public int highScore;
    public int fastestTime; // clear time in milliseconds; used for leaderboards
}