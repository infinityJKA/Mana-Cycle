using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class Authentication {
    static bool signingIn = false;
    public static async Task Authenticate() {
        if (signingIn) {
            Debug.LogWarning("Already in the process of signing in");
        }

        signingIn = true;

        if (UnityServices.State == ServicesInitializationState.Uninitialized)  {
            await UnityServices.InitializeAsync();
        }

        if (AuthenticationService.Instance.IsSignedIn) {
            Debug.LogWarning("Already signed in");
            signingIn = false;
            return;
        }

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in with id "+AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        signingIn = false;
    }
}