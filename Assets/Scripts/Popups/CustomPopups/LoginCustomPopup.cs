// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class LoginCustomPopup : CustomPopup
// {
//     public void LoginGuestPressed() {
//         PlayerManager.LoginGuest();
//     }

//     public void LoginSteamPressed() {
//         PlayerManager.LoginSteam();
//     }

//     // Is run after the login process is completed whether successful or not.
//     public void AfterLogin() {
//         if (PlayerManager.loggedIn) {
//             CloseThisPopup();
//         } else {
//             // todo: show an error text on tje login window saying osmething like "failed to log in" 
//             // and maybe provide info based on context.
//             Debug.LogWarning("Login menu login process failed");
//         }
//     }
// }