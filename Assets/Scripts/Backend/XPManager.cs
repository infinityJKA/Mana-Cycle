using LootLocker.Requests;
using UnityEngine;

public class XPManager {
    public static int level {get; private set;} = 1;
    public static int xp {get; private set;} = 0;
    public static int xpToNext {get; private set;} = 100;

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
                level = (int)response.level;
                xp = (int)response.xp;
                xpToNext = (int)response.level_thresholds.next;
            } else { // player has not earned any xp yet
                level = 1;
                xp = 0;
                xpToNext = 100;
            }

            if (SidebarUI.instance) SidebarUI.instance.UpdateXPDisplay();
        });
    }
}