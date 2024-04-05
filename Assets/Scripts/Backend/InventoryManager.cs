using UnityEngine;
using LootLocker;
using LootLocker.Requests;

public class InventoryManager {
    public static void GetInventory() {
        LootLockerSDKManager.GetInventory((response) =>
        {
            if (!response.success)
            {
                Debug.LogError("Error getting player inventory: "+response.errorData.message);
            }
        });
    }
}