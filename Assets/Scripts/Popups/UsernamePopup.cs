using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class UsernamePopup : BasicPopup {
    [SerializeField] private TMP_InputField usernameInput;

    [SerializeField] private Color errorColor;

    public override void OnShow()
    {
        usernameInput.text = PlayerManager.playerUsername;
    }

    public void SetNamePressed() {
        SetName(usernameInput.text);
    }
    
    // referenced in usernameInput's OnSubmit UnityEvent on prefab
    public void OnUsernameSubmit(BaseEventData data)
    {
        if (usernameInput.wasCanceled)
            return;
    
        SetName(usernameInput.text);
    }

    // called from UsernamePopup submit
    // will call toast messages if username is not valid
    public void SetName(string username) {
        Debug.Log("inputted username: "+username);
        if (username.Length == 0) {
            description = "Enter a username";
            descriptionLabel.color = errorColor;
            return;
        } else if (username.Length < 3 || username.Length > 16) {
            description = "Username must be 3-16 characters";
            descriptionLabel.color = errorColor;
            return;
        } else if (username == PlayerManager.playerUsername) {
            Close();
            return;
        }

        // maybe want to verify that username is only alphanumeric, though TMP input only allows alphanumeric, and lootlocker should error for that anyways

        usernameInput.interactable = false;
        confirmButton.interactable = false;

        PlayerManager.SetPlayerName(username, (response) => {
            if (!response.success) {
                description = response.errorData.message;
                descriptionLabel.color = errorColor;
                usernameInput.interactable = true;
                confirmButton.interactable = true;
                return;
            }

            ToastManager.ShowToast("Username changed successfully", status: Status.Success);
            Close();
        });
    }
}