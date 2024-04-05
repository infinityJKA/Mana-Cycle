using System;
using System.Collections.Generic;
using System.IO;
using LootLocker.Requests;
using UnityEngine;

[Serializable]
public class MatchHistory {
    public static MatchHistory current {get; private set;}

    // ===== Saving & loading
    public const string fileName = "matchhistory.json";
    public static readonly string filePath;
    static MatchHistory() {
        filePath = Path.Join(Application.persistentDataPath, fileName);
        Load();
    }
    public static void Save() {
        FileStorageManager.Save(current, filePath, encrypt: false);

        // if logged in, save on backend
        if (LootLockerSDKManager.CheckInitialized()) {
            RemoteFileManager.UploadFile(filePath, fileName, isPublic: false);
        }
    }
    public static void Load() {
        current = FileStorageManager.Load<MatchHistory>(filePath, decrypt: false);
    }

    /// <summary>
    /// Stored stats for solo matches. up to 10 can be stored
    /// </summary>
    public List<SoloHistoryEntry> soloHistory = new List<SoloHistoryEntry>();

    /// <summary>
    /// Combined match stats over ALL matches played ever by the player.
    /// Keeps track of the highest individual stats out of any match played.
    /// used for achievement progress bars.
    /// </summary>
    public MatchStats highestStats = new MatchStats();

    /// <summary>
    /// Totals kept across all matches played.
    /// </summary>
    public MatchTotals aggregatedStats = new MatchTotals();

    private const int soloHistoryLengthLimit = 10;
    public void AddSoloEntry(SoloHistoryEntry entry) {
        while (soloHistory.Count >= soloHistoryLengthLimit) {
            soloHistory.RemoveAt(0);
        }
        soloHistory.Add(entry);

        highestStats.Merge(entry.stats);
        aggregatedStats.Aggregate(entry.stats);

        Save();
    }
}


[Serializable]
public class MatchTotals {
    /// <summary>Total amount of points earned (solo) or damage dealt (versus).
    public int totalScore;
    /// <summary>Total amount of mana this board has cleared</summary>
    public int totalManaCleared;
    /// <summary>Total amount of spellcasts this player has performed */</summary>
    public int totalSpellcasts;
    /// <summary>Total amount of spellcast chains this player has started (aka. pressing spellcast and getting a chain of at least 1)</summary>
    public int totalManualSpellcasts;
    /// <summary>Total amount of damage countered (versus matches only)</summary>
    public int totalDamageCountered;

    /// <summary>
    /// Aggregate new match data into this match totals. Used for aggregatedStats for total running stats across all matches ever played by the player.
    /// </summary>
    public void Aggregate(MatchTotals other) {
        totalScore += other.totalScore;
        totalManaCleared += other.totalManaCleared;
        totalSpellcasts += other.totalSpellcasts;
        totalManualSpellcasts += other.totalManualSpellcasts;
        totalDamageCountered += other.totalDamageCountered;
    }
}

[Serializable]
public class MatchStats : MatchTotals {
      /// <summary> Highest combo performed by the player </summary>
    public int highestCombo;
    /// <summary>Highest cascade performed by the player </summary>
    public int highestCascade;
    /// <summary>Highest single damage dealt in one spellcast during the battle </summary>
    public int highestSingleDamage;

    // Used by highestStats to merge best stats between this and the new incoming next match.
    public void Merge(MatchStats other) {
        totalScore = Math.Max(totalScore, other.totalScore);
        totalManaCleared = Math.Max(totalManaCleared, other.totalManaCleared);
        totalSpellcasts = Math.Max(totalSpellcasts, other.totalSpellcasts);
        highestCombo = Math.Max(highestCombo, other.highestCombo);
        highestCascade = Math.Max(highestCascade, other.highestCascade);
        totalManualSpellcasts = Math.Max(totalManualSpellcasts, other.totalManualSpellcasts);
        highestSingleDamage = Math.Max(highestSingleDamage, other.highestSingleDamage);
        totalDamageCountered = Math.Max(totalDamageCountered, other.totalDamageCountered);
    }
}

[Serializable]
public class SoloHistoryEntry {
    public string levelId;
    public string battlerId;
    public long date;
    public MatchStats stats;
}

// coming soon maybe: versus history entries