using UnityEngine;
using LootLocker.Requests;

public class PlayerManager {
    public static string playerId {get; private set;}
    public static bool loginInProgress {get; private set;} = false;
    public static bool loginAttempted {get; private set;} = true;
    public static bool loggedIn {get; private set;} = false;

    public static void LoginGuest() {
        if (loggedIn || loginInProgress) return;

        loginInProgress = true;
        LootLockerSDKManager.StartGuestSession((response) => {
            if (response.success)
            {
                Debug.Log("Guest player logged in");
                playerId = response.player_id.ToString();
                loggedIn = true;
            } else {
                Debug.Log("Could not log in as guest: "+response.errorData);
            }
            loginAttempted = true;
            loginInProgress = false;
        });
    }
}