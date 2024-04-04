using TMPro;
using UnityEngine;
public class UsernamePopup : BasicPopup {
    [SerializeField] private TMP_InputField usernameInput;

    public void SetNamePressed() {
        SetName(usernameInput.text);
    }

    // called from UsernamePopup submit
    // will call toast messages if username is not valid
    public void SetName(string username) {
        if (username.Length == 0) {
            ToastManager.ShowToast("Enter a username");
        } else if (username.Length < 3 || username.Length > 16) {
            ToastManager.ShowToast("Username must be 3-16 characters");
        }

        // todo: verify that username is only alphanumeric

        PlayerManager.SetPlayerName(username, (response) => {
            if (!response.success) {
                ToastManager.ShowToast(response.errorData.message, status: Status.Error);
                return;
            }

            ToastManager.ShowToast("Username changed successfully", status: Status.Success);
            Close();
        });
    }
}