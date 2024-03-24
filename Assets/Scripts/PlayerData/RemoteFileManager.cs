using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LootLocker.Requests;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles the retrieving and uploading of player files to LootLocker.
/// Also handles the merging of local and online data if they do not match.
/// </summary>
public class RemoteFileManager {
    public static Dictionary<string, int> fileIds {get; private set;}

    public static bool remoteProgressLoaded = false;

    static RemoteFileManager() {
        fileIds = new Dictionary<string, int>();
    }

    public static void UploadFile(string filePath, string name, bool isPublic) {
        FileStream fileStream = File.Open(filePath, FileMode.Open);

        // if the file exists, update it
        // otherwise, upload it

        if (fileIds.TryGetValue(name, out int id)) {
            LootLockerSDKManager.UpdatePlayerFile(id, fileStream, OnFileUpload);
        } else {
            LootLockerSDKManager.UploadPlayerFile(fileStream, name, isPublic, OnFileUpload);
        }
    }

    public static void OnFileUpload(LootLockerPlayerFile response) {
        if (!response.success)
        {
            Debug.LogError("Error uploading/updating player file");
        } 

        Debug.Log("Successfully uploaded player file, url: " + response.url);
        fileIds[response.name] = response.id;
    }

    public static void GetPlayerFiles(bool download) {
        LootLockerSDKManager.GetAllPlayerFiles((response) => {
            if (!response.success) {
                Debug.LogError("Error retrieving player files: "+response.errorData);
            }

            // Start the downloads
            foreach (var item in response.items) {
                fileIds[item.name] = item.id;

                if (download && item.name == "progressdata.sav") {
                    DownloadSerializedData<ProgressData>(item.url, incoming => {
                        HandleProgressDataMerge(ProgressData.current, incoming);
                    });
                }
            }
        });
    }

    public static void DownloadSerializedData<T>(string url, System.Action<T> downloadedAction) {
        UnityWebRequest request = new UnityWebRequest(url)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        var requestAsync = request.SendWebRequest();
        requestAsync.completed += (response) => {
            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError("Error downloading file");
                return;
            }

            Debug.Log("Downloaded "+url);

            BinaryFormatter converter = new BinaryFormatter();
            T data;
            using (MemoryStream memoryStream = new MemoryStream(request.downloadHandler.data)) {
                data = (T)converter.Deserialize(memoryStream);
            }
            downloadedAction.Invoke(data);
        };
    }

    public static void HandleProgressDataMerge(ProgressData current, ProgressData incoming) {
        remoteProgressLoaded = true;

        // If the two are exactly the same, no merge required, local data is same as remote data.
        if (current.Equals(incoming)) return;

        // if not equal:
        // May want to prompt player to choose either local or online.
        // However, level data can be merged pretty easily by just combining current and incoming level progress and choosing the greater of the two.
        // so no need to ask until data may get more complex and actual progress may get overwritten.
        Debug.LogWarning("Local and remote progress are not equal. Merging progress data...");
        MergeProgressData(current, incoming);

        // save the newly merged data to local file.
        ProgressData.Save();
    }

    public static void MergeProgressData(ProgressData current, ProgressData incoming) {
        // run for each incoming level being merged into current
        foreach (var kvp in incoming.levels) {
            string levelId = kvp.Key;
            LevelData incomingLevel = kvp.Value;

            // if already have clear status for level, select the highest high score and lowest clear time from both current and incoming
            if (current.levels.TryGetValue(levelId, out LevelData currentLevel)) {
                if (currentLevel.highScore < incomingLevel.highScore) {
                    currentLevel.highScore = incomingLevel.highScore;
                    Debug.Log(levelId+" score: "+currentLevel.highScore+" -> "+incomingLevel.highScore);
                    LeaderboardManager.UploadScore(levelId+"_Score", incomingLevel.highScore);
                }
                if (currentLevel.fastestTime > incomingLevel.fastestTime) {
                    currentLevel.fastestTime = incomingLevel.fastestTime;
                    Debug.Log(levelId+" time: "+currentLevel.fastestTime+" -> "+incomingLevel.fastestTime);
                }
                current.levels[levelId] = currentLevel;
            } 
            // if not, get the clear data from incoming
            else {
                current.levels[levelId] = incomingLevel;
                Debug.Log(levelId+" is now cleared");
                LeaderboardManager.UploadScore(levelId+"_Score", incomingLevel.highScore);
            }
        }

        // merge achievements earned
        foreach (string achievementId in incoming.achievements) {
            if (!current.achievements.Contains(achievementId)) {
                current.achievements.Add(achievementId);
                Debug.Log(achievementId+" now earned");
            }
        }
    }
}