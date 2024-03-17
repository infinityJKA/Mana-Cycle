using System.Collections;
using LootLocker.Requests;
using SoloMode;
using UnityEngine;

// monobehaviour on the PlayerManager.
public class LeaderboardManager {
    public static bool uploadingScore = false;

    public static void UploadScore(Level level, int score) {
        if (uploadingScore) return;
        uploadingScore = true;

        string leaderboardKey = level.levelId+"_Score";
        uploadingScore = true;
        LootLockerSDKManager.SubmitScore(PlayerManager.playerId, score, leaderboardKey, (response) => {
            if (response.success) {
                Debug.Log("Successfully uploaded score to "+leaderboardKey);
            } else {
                Debug.Log("Failed to upload score to "+leaderboardKey+": "+response.errorData);
            }
            uploadingScore = false;
        });
    }
}