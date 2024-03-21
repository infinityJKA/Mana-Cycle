using System;
using System.Collections;
using System.Collections.Generic;
using LootLocker.Requests;
using SoloMode;
using UnityEngine;

// monobehaviour on the PlayerManager.
public class LeaderboardManager {
    public static bool uploadingScore = false;

    // all loaded data, stored in memory (willprobably clear from mem once solomode scene is left)
    public static Dictionary<Level, Dictionary<LeaderboardType, LeaderboardEntryList>> data;

    static LeaderboardManager() {
        data = new Dictionary<Level, Dictionary<LeaderboardType, LeaderboardEntryList>>();
    }

    private static void UploadScore(string key, int score) {
        if (uploadingScore) return;
        uploadingScore = true;

        uploadingScore = true;
        LootLockerSDKManager.SubmitScore(PlayerManager.playerID, score, key, (response) => {
            if (response.success) {
                Debug.Log("Successfully uploaded score to "+key);
            } else {
                Debug.Log("Failed to upload score to "+key+": "+response.errorData);
            }
            uploadingScore = false;
        });
    }

    // page starts from 0
    private static void GetScores(string key, int page, Action<LootLockerGetScoreListResponse> itemsReceived) {
        LootLockerSDKManager.GetScoreList(key, 10, page*10, (response) => {
            if (!response.success) {
                Debug.Log("Failed to retrieve scores from "+key+": "+response.errorData);
            }
            itemsReceived.Invoke(response);
        });
    }

    public static void EnsureContainersExist(Level level, LeaderboardType type) {
        if (!data.ContainsKey(level)) data[level] = new Dictionary<LeaderboardType, LeaderboardEntryList>();
        if (!data[level].ContainsKey(type)) data[level][type] = new LeaderboardEntryList();

    }

    public static void UploadLeaderboardScore(Level level, int score) {
        // lootlocker
        UploadScore(level.scoreLeaderboardKey, score);
    }

    public static void UploadLeaderboardTime(Level level, int score) {
        // lootlocker
        UploadScore(level.timeLeaderboardKey, score);
    }

    public struct LeaderboardDataCallback {
        public bool success;
        public Level level;
        public LeaderboardType type;
        public int page;
        public string errorMsg;
    }

    // int sent to callback is the page that was loaded
    public static void LoadLeaderboardData(Level level, LeaderboardType type, int page, Action<LeaderboardDataCallback> onLoaded) {
        switch (type) {
            case LeaderboardType.ScoreGlobal:
            case LeaderboardType.TimeGlobal:
                string key = type == LeaderboardType.ScoreGlobal ? level.scoreLeaderboardKey : level.timeLeaderboardKey;
                var entryList = GetEntryList(level, type);
                entryList.loadingPage = page;
                GetScores(key, page*10, (response) => {
                    if (response.success) {
                        entryList.AddPage(page, response.items);
                    } 
                    
                    onLoaded.Invoke(new LeaderboardDataCallback(){
                        success = response.success,
                        level = level,
                        type = type,
                        page = page,
                    });
                });
                break;
        }
    }

    public static LootLockerLeaderboardMember[] RetrieveLoadedData(Level level, LeaderboardType type, int page) {
        return data[level][type].pages[page];
    }

    public static bool IsDataLoaded(Level level, LeaderboardType type, int page) {
        return GetEntryList(level, type).HasPage(page);
    }

    public static LeaderboardEntryList GetEntryList(Level level, LeaderboardType type) {
        EnsureContainersExist(level, type);
        return data[level][type];
    }

    public static void RemoveAllLoadedEntries() {
        foreach (var kvp in data) {
            foreach (var entryListKvp in kvp.Value) {
                entryListKvp.Value.pages.Clear();
                entryListKvp.Value.loadingPage = -1;
            }
        }
    }
}

public class LeaderboardEntryList {
    public Dictionary<int, LootLockerLeaderboardMember[]> pages = new Dictionary<int, LootLockerLeaderboardMember[]>();

    // page currently being loaded - cannot load more than one at once.
    public int loadingPage = -1;

    /// <summary>
    /// Adds or overwrites the given page.
    /// </summary>
    public void AddPage(int page, LootLockerLeaderboardMember[] items) {
        pages[page] = items;
    }

    public bool HasPage(int page) {
        return pages.ContainsKey(page);
    }
}

public enum LeaderboardType {
    // local files? not implemented
    ScoreLocal,
    TimeLocal,

    // steam friends - not implemented
    ScoreFriends,
    TimeFriends,

    // lootlocker global leaderboards
    ScoreGlobal,
    TimeGlobal // not implemented
}