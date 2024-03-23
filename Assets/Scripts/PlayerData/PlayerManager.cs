using UnityEngine;
using LootLocker.Requests;
using Steamworks;
using System;

public class PlayerManager {
    public static string playerID {get; private set;}
    public static string playerUsername {get; private set;}

    public static bool loginInProgress {get; private set;} = false;
    public static bool loginAttempted {get; private set;} = true;

    /// <summary>
    /// If player is logged in ONLINE. does not include local (offline) mode login.
    /// </summary>
    public static bool loggedIn {get; private set;} = false;
    public static bool isOffline => loginMode == LoginMode.Local;

    public static LoginMode loginMode {get; private set;}
    public enum LoginMode {
        Local, // data saved locally, basically an offline mode. username may come from last login session (not implemented yet)
        Guest,
        Steam
    }

    static PlayerManager() {
        // Platform specific auto login on start
        // stop if already logged in online or logging in
        if (loggedIn || loginInProgress) return;

        // login locally first - will show cached data that may be saved (will be instant, no networking required)
        LoginLocal();

        // after local login, attempt to login online which may take a bit.
        // if this fails the player will remain logged in locally.
        if (SteamManager.Initialized) {
            LoginSteam();
        } else {
            LoginGuest();
        }
    }

    // Not much of a login, but info from file about previous session and displays that.
    // While in local mode, a notifier wills how up on the sidebar showing that the player is currently offline.
    public static void LoginLocal() {
        playerUsername = FBPP.GetString("playerUsername", "");
        OnLoginFinished();
    }

    /// <param name="next">Action to run after login process is complete, whether successful or not</param>
    public static void LoginGuest() {
        if (loggedIn || loginInProgress) return;

        loginMode = LoginMode.Guest;
        loginInProgress = true;
        loginAttempted = true;
        LootLockerSDKManager.StartGuestSession((response) => {
            if (response.success)
            {
                Debug.Log("Guest player logged in");
                playerID = response.player_ulid;
                playerUsername = "Guest "+response.player_id;
                loggedIn = true;
            } else {
                Debug.Log("Could not log in as guest: "+response.errorData);
            }
            OnLoginFinished();
        });
    }

    /// <param name="next">Action to run after login process is complete, whether successful or not</param>
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
                OnLoginFinished();
                return;
            }

            CSteamID SteamID = SteamUser.GetSteamID();
            LootLockerSDKManager.StartSteamSession(SteamID.ToString(), (response) =>
            {
                if (!response.success)
                {
                    Debug.Log("error starting sessions");     
                    return;
                }

                loggedIn = true;
                playerID = response.player_ulid;
                playerUsername = SteamFriends.GetPersonaName();
                Debug.Log("steam session started!");
                OnLoginFinished();
            });
        });
    }

    // function to be run after login process is fiinished (successful or not).
    // If logged in, Retreive stuff like the wallet and such when first logging in.
    private static void OnLoginFinished() {
        loginInProgress = false;
        if (SidebarUI.instance) SidebarUI.instance.UpdatePlayerInfo();
        if (loggedIn) {
            WalletManager.GetWallet();
            XPManager.GetPlayerInfo();
        }
    }  
}