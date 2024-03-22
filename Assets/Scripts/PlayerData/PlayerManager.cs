using UnityEngine;
using LootLocker.Requests;
using Steamworks;

public class PlayerManager {
    public static string playerID {get; private set;}
    public static string playerUsername {get; private set;}

    public static bool loginInProgress {get; private set;} = false;
    public static bool loginAttempted {get; private set;} = true;
    public static bool loggedIn {get; private set;} = false;

    public static LoginMode loginMode {get; private set;}
    public enum LoginMode {
        None,
        Guest,
        Steam
    }

    public static void LoginGuest() {
        if (loggedIn || loginInProgress) return;

        loginMode = LoginMode.Guest;
        loginInProgress = true;
        loginAttempted = true;
        LootLockerSDKManager.StartGuestSession((response) => {
            loginInProgress = false;
            if (response.success)
            {
                Debug.Log("Guest player logged in");
                playerID = response.player_ulid;
                playerUsername = "Guest "+response.player_ulid;
                loggedIn = true;
                OnLoginSetup();
            } else {
                Debug.Log("Could not log in as guest: "+response.errorData);
            }
        });
    }

    public static void LoginSteam() {
        // To make sure Steamworks.NET is initialized
        if (!SteamManager.Initialized) {
            // TODO: show error popup
            return;
        }

        loginMode = LoginMode.Steam;
        loginInProgress = true;
        loginAttempted = true;

        var ticket = new byte[1024];
        var networkIdentity = new SteamNetworkingIdentity();
        var newTicket = SteamUser.GetAuthSessionTicket(ticket, 1024, out uint ticketSize, ref networkIdentity);
        string steamSessionTicket = LootLockerSDKManager.SteamSessionTicket(ref ticket, ticketSize);

        LootLockerSDKManager.VerifySteamID(steamSessionTicket, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("Error verifying Steam ID: " + response.errorData.message);
                loginInProgress = false;
                return;
            }

            CSteamID SteamID = SteamUser.GetSteamID();
            LootLockerSDKManager.StartSteamSession(SteamID.ToString(), (response) =>
            {
                loginInProgress = false;
                if (!response.success)
                {
                    Debug.Log("error starting sessions");     
                    return;
                }

                loggedIn = true;
                playerID = response.player_ulid;
                playerUsername = SteamFriends.GetPersonaName();
                Debug.Log("steam session started!");
                OnLoginSetup();
            });
        });
    }

    // Retreive stuff like the wallet and such when first logging in.
    public static void OnLoginSetup() {
        if (SidebarUI.instance) SidebarUI.instance.ShowPlayerInfo();

        // TODO: hide currency (or whole panel altogether) while data is still being fetched
        SidebarUI.instance.SetCoins("0");
        SidebarUI.instance.SetIridium("0");
        WalletManager.GetWallet();
    }  
}