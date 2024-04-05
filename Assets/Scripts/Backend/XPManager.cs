using System;
using LootLocker.Requests;
using UnityEngine;

public class XPManager {
    public static ulong level {get; private set;} = 1;
    public static ulong xp {get; private set;} = 0;
    public static ulong xpToNext {get; private set;} = 100;

    public static void GetPlayerInfo() {
        if (!PlayerManager.loggedIn) return;

        LootLockerSDKManager.GetPlayerInfo((response) =>
        {
            if (!response.success)
            {
                Debug.LogError("Error getting player info: " + response.errorData);
                return;
            }

            if (response.level.HasValue) {
                level = (ulong)response.level;
                xp = (ulong)response.xp;
                xpToNext = (ulong)response.level_thresholds.next;
            } else { // player has not earned any xp yet
                level = 1;
                xp = 0;
                xpToNext = 100;
            }

            if (SidebarUI.instance) SidebarUI.instance.UpdateXPDisplay();
        });
    }

    public static void AddPoints(string progression, ulong amountOfPoints) {
        LootLockerSDKManager.AddPointsToPlayerProgression(progression, amountOfPoints, response =>
        {
            if (!response.success) {
                Debug.LogError("Failed to add points: " + response.errorData.message);
                return;
            }

            if (progression == "playerlevel") {
                level = response.step;
                xp = response.points;
                if (response.next_threshold.HasValue) xpToNext = response.next_threshold.Value;
            }

            Debug.Log($"Gained {amountOfPoints} points in progression {progression}!");

            // If the awarded_tiers array contains any items that means the player leveled up
            // There can also be multiple level-ups at once
            if (response.awarded_tiers.Count > 0)
            {
                foreach (var awardedTier in response.awarded_tiers)
                {
                    Debug.Log($"Reached level {awardedTier.step}!");

                    foreach (var assetReward in awardedTier.rewards.asset_rewards)
                    {
                        Debug.Log($"Rewarded with an asset, id: {assetReward.asset_id}!");
                    }
                    
                    foreach (var progressionPointsReward in awardedTier.rewards.progression_points_rewards)
                    {
                        Debug.Log($"Rewarded with {progressionPointsReward.amount} bonus points in {progressionPointsReward.progression_name} progression!");
                    }
                    
                    foreach (var progressionResetReward in awardedTier.rewards.progression_reset_rewards)
                    {
                        Debug.Log($"Progression {progressionResetReward.progression_name} has been reset as a reward!");
                    }

                    foreach (var currencyReward in awardedTier.rewards.currency_rewards)
                    {
                        Debug.Log($"Rewarded with {currencyReward.amount} {currencyReward.currency_name}!");

                        if (currencyReward.currency_code == "idm") {
                            WalletManager.iridium += int.Parse(currencyReward.amount);
                        } else if (currencyReward.currency_code == "ibn") {
                            WalletManager.coins += int.Parse(currencyReward.amount);
                        }
                    }
                }
            }
        });
    }

    public static void AddXP(ulong xp) {
        AddPoints("playerlevel", xp);
    }
}

/// <summary>
/// Represents a level series progression on LootLocker backend and handles clearing levels on it.
/// Instances are serialized & referenced within the SoloMode.SoloMenuTab scriptable object for that level series.
/// </summary>
[Serializable]
public class SoloLevelSeriesProgression {
    [SerializeField] private string progressionKey;

    public bool retrieved {get; private set;} = false;

    public ulong currentLevel {get; private set;}

    // may be set externally, should clear this level once level is retrieved
    public ulong desiredLevel {get; set;} = 0;

    public SoloLevelSeriesProgression(string key) {
        progressionKey = key;
    }

    public void Retrieve() {
        if (progressionKey == null || progressionKey == "") {
            // this probably doesn't need to be a warning but putting it here incase it helps with debugging cause idk what im doing
            Debug.LogWarning("Trying to submit score to series with no progression key.");
            return;
        }

        LootLockerSDKManager.GetPlayerProgression(progressionKey, response =>
        {
            if (response.success) {
                currentLevel = response.step;
                // Output the player level and show how much points are needed to progress to the next tier
                Debug.Log($"The player is currently level {response.step} in progression {progressionKey}");
            } else {
                if (response.statusCode == 404) {
                    Debug.Log("Player progression not found - so effectively at level 0. will still submit score if desiredLevel > 0");
                } else {
                    Debug.Log("Failed to retrieve player progression: " + response.errorData.message);
                    return;
                }
            }
            
            retrieved = true;

            if (desiredLevel > 0) {
                SetLevelCleared(desiredLevel, verifyLevel: false);
            }
        });
    }

    /// <summary>
    /// Send to LootLocker to set the level passed as cleared in the current series of solo mode levels.
    /// If level is not retrieved, it will be retrieved first, to be sure that level is being set accurately.
    /// </summary>
    /// <param name="level">target level to be cleared up to</param>
    public void SetLevelCleared(ulong level, bool verifyLevel = true) {
        if (verifyLevel) {
            retrieved = false;
        }
        if (!retrieved) {
            desiredLevel = level;
            Retrieve();
            return;
        }
        
        if (level > currentLevel) {
            ulong amount = level - currentLevel;
            XPManager.AddPoints(progressionKey, amount);
        }
    }
}