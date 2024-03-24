using LootLocker.Requests;
using UnityEngine;

/// <summary>
/// Handles the retrieving and uploading of player files to LootLocker.
/// Also handles the merging of local and online data if they do not match.
/// </summary>
public class PlayerFileManager {
    public void DownloadAllPlayerFiles() {
        LootLockerSDKManager.GetAllPlayerFiles((response) => {
            if (!response.success) {
                Debug.LogError("Error retrieving player files: "+response.errorData);
            }
        });
    }
}