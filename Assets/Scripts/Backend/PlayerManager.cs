using UnityEngine;
using LootLocker.Requests;
using Steamworks;
using System;

// Handles logging in and logging out to player accounts.
public class PlayerManager {
    public static string playerID {get; private set;}
    public static string playerUsername {get; private set;}

    public static bool loginInProgress {get; private set;} = false;
    public static bool loginAttempted {get; private set;} = true;
    public static string loginError {get; set;} = "";

    /// <summary>
    /// If player is logged in ONLINE. does not include local (offline) mode login.
    /// </summary>
    public static bool loggedIn {get; private set;} = false;

    public static LoginMode loginMode {get; private set;}
    public enum LoginMode {
        None, // not logged in
        Guest,
        Steam
    }

    static PlayerManager() {
        // Platform specific auto login on start
        // stop if already logged in online or logging in
        if (loggedIn || loginInProgress) return;
        if (SteamManager.Initialized) {
            LoginSteam();
        } else {
            LoginGuest();
        }
    }

    public static void LoginGuest() {
        if (loggedIn || loginInProgress) return;

        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            OnLoginFinished();
            return;
        }

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
                loginError = "Login failed";
                Debug.Log("Could not log in as guest: "+response.errorData);
            }
            OnLoginFinished();
        });
    }

    public static void LoginSteam() {
        // To make sure Steamworks.NET is initialized
        if (!SteamManager.Initialized) {
            loginError = "Steam not initialized";
            OnLoginFinished();
            return;
        }

        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            OnLoginFinished();
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
                loginError = "Steam ID verification error";
                Debug.Log("Error verifying Steam ID: " + response.errorData.message);
                OnLoginFinished();
                return;
            }

            CSteamID SteamID = SteamUser.GetSteamID();
            LootLockerSDKManager.StartSteamSession(SteamID.ToString(), (response) =>
            {
                if (!response.success)
                {
                    loginError = "Failed to start session";
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

    // function to be run after login (OR LOGOUT) process is fiinished (successful or not).
    // If logged in, Retreive stuff like the wallet and such when first logging in.
    private static void OnLoginFinished() {
        loginInProgress = false;
        
        if (SidebarUI.instance) {
            SidebarUI.instance.UpdatePlayerInfo();
            SidebarUI.instance.UpdateButtonsWindow();
        }

        if (CosmeticShop.instance) CosmeticShop.instance.OnConnected();

        if (loggedIn) {
            WalletManager.GetWallet();
            XPManager.GetPlayerInfo();
            RemoteFileManager.GetPlayerFiles(download: true);
        }
    }  

    public static void Logout() {
        LootLockerSDKManager.EndSession((response) => {
            loggedIn = false;

            // always log out regardless if the server says error or not. (but still show the error)
            if (!response.success) {
                Debug.LogError("error ending session: "+response.errorData.message);
            }
            
            OnLoginFinished();
            if (SidebarUI.instance) SidebarUI.instance.SelectLastSelected();
        });
    }
}