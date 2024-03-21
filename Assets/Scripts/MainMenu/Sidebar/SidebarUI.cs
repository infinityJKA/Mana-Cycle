using TMPro;
using UnityEngine;

public class SidebarUI : MonoBehaviour {
    public static SidebarUI instance {get; private set;}

    [SerializeField] private Animator animator;

    // windows
    [SerializeField] private GameObject playerInfoWindow, loginOptionsWindow;

    // Shown while logged in
    [SerializeField] private TMP_Text usernameLabel, coinCountLabel, iridiumCountLabel;

    

    private void Start() {
        instance = this;

        if (PlayerManager.loggedIn) {
            ShowPlayerInfo();
        } else {
            ShowLoginOptions();
        }
    }

    public void SetExpanded(bool expanded) {
        animator.SetBool("expanded", expanded);
    }

    public void ShowLoginOptions() {
        playerInfoWindow.SetActive(false);
        loginOptionsWindow.SetActive(true);
    }

    public void ShowPlayerInfo() {
        loginOptionsWindow.SetActive(false);
        playerInfoWindow.SetActive(true);
    }

    public void LoginGuestPressed() {
        PlayerManager.LoginGuest();
    }

    public void SetCoins(string amount) {
        coinCountLabel.text = amount;
    }

    public void SetIridium(string amount) {
        iridiumCountLabel.text = amount;
    }
}