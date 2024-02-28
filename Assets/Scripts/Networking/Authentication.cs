using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System;

public class Authentication {
    static bool signingIn = false;
    public static async Task Authenticate() {
        if (signingIn) {
            Debug.LogWarning("Already in the process of signing in");
        }

        signingIn = true;

        if (UnityServices.State == ServicesInitializationState.Uninitialized)  {
            try {
                await UnityServices.InitializeAsync();
            } catch (Exception e) {
                PopupManager.instance.ShowBasicPopup("Error", e.ToString());
            }
        }

        if (AuthenticationService.Instance.IsSignedIn) {
            Debug.LogWarning("Already signed in");
            signingIn = false;
            return;
        }

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in with id "+AuthenticationService.Instance.PlayerId);
        };
        try {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        } catch (Exception e) {
            PopupManager.instance.ShowBasicPopup("Authentication Error", e.ToString());
        }

        signingIn = false;
    }
}