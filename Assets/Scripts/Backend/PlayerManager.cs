using UnityEngine;
using LootLocker.Requests;

using Cosmetics;
using System;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

// Handles logging in and logging out to player accounts.
public class PlayerManager {
    public static string playerID {get; private set;}
    public static int playerLegacyId {get; private set;}

    // name shown in the sidebar.
    public static string playerUsername {get; private set;}
    // if using a display name that the user has set via guest mode.
    public static bool usingDisplayName {get; private set;} = false;
    // true in guest mode; false in steam mode
    public static bool canChangeUsername {get; private set;} = false;

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
        LoginIfNotLoggedIn();
    }

    // ======== LOGIN /AUTHENTICATION
    public static void LoginIfNotLoggedIn() {
        if (LootLockerSDKManager.CheckInitialized()) return;

        // in case of domain reload disabled
        playerID = "";
        loggedIn = false;
        loginInProgress = false;
        loginAttempted = false;
        CatalogManager.paletteColors.Reset();
        CatalogManager.iconPacks.Reset();

        // Platform specific auto login on start
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
                playerLegacyId = response.player_id;
                canChangeUsername = true;
                loggedIn = true;
            } else {
                loginError = "Login failed";
                Debug.Log("Could not log in as guest: "+response.errorData);
            }
            OnLoginFinished();
        });
    }


    public static void LoginSteam() {
        #if !DISABLESTEAMWORKS
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
                usingDisplayName = false;
                canChangeUsername = false;
                Debug.Log("steam session started!");
                OnLoginFinished();
            });
        });
        #endif
    }

    // function to be run after login (OR LOGOUT) process is fiinished (successful or not).
    // If logged in, Retreive stuff like the wallet and such when first logging in.
    private static void OnLoginFinished() {
        loginInProgress = false;
        
        if (SidebarUI.instance) {
            SidebarUI.instance.UpdatePlayerInfo();
            SidebarUI.instance.UpdateButtonsWindow();
        }

        if (loggedIn) {
            WalletManager.GetWallet();
            XPManager.GetPlayerInfo();
            XPManager.AddXP(0); // If progression hasn't started, this will start it. progression first auto tier has 2500 ibn for starters money
            InventoryManager.GetInventory();
            RemoteFileManager.GetPlayerFiles(download: true);

            playerUsername = "Loading...";
            RetrievePlayerName();
        }  

        if (CosmeticShop.instance) CosmeticShop.instance.OnConnected();
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

    // ======== USERNAMES
    // action will be called with either sucess true or false.
    // (used by username change popup)
    public static void SetPlayerName(string name, Action<PlayerNameResponse> action) {
        if (!canChangeUsername) return;
        
        LootLockerSDKManager.SetPlayerName(name, (response) =>
        {
            action.Invoke(response);

            if (!response.success)
            {
                Debug.LogError("Error setting player name: "+response.errorData.message);
                return;
            }

            Debug.Log("Successfully set player name to "+name);
            playerUsername = name;
            usingDisplayName = true;
            if (SidebarUI.instance) {
                SidebarUI.instance.UpdatePlayerInfo();
            }
        });
    }

    public static void RetrievePlayerName() {
        LootLockerSDKManager.GetPlayerName((response) =>
        {
            if (!response.success)
            {
                Debug.Log("Error getting player name");
                return;
            }

            if (response.name != null && response.name.Length > 0) {
                playerUsername = response.name;
                usingDisplayName = true;
            } else {
                playerUsername = "Guest "+playerLegacyId;
                usingDisplayName = false;
            }
            
            if (SidebarUI.instance) {
                SidebarUI.instance.UpdatePlayerInfo();
            }
        });
    }
}