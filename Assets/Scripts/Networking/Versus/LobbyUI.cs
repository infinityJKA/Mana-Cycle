using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour {
    [SerializeField] private TMPro.TMP_InputField matchCodeInput;
    [SerializeField] private Button joinButton, hostButton;

    public void Host() {
        matchCodeInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;
    }

    public void Join() {
        matchCodeInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;
    }
}