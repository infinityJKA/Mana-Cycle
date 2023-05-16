using UnityEngine;
using UnityEngine.UI;

namespace MainMenu {
    /// <summary>
    /// Controls the settings menu, currently only the close button. (Slider volumes are handled solely by SoundManager)
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private InputScript inputScript;
        [SerializeField] private Toggle ghostPieceToggle;
        [SerializeField] private Button closeButton;
        
        void Start() {
            if (ghostPieceToggle) {
                if (!PlayerPrefs.HasKey("drawGhostPiece")) PlayerPrefs.SetInt("drawGhostPiece", 1);
                ghostPieceToggle.isOn = PlayerPrefs.GetInt("drawGhostPiece") == 1;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(inputScript.Pause)) {
                closeButton.onClick.Invoke();
            }
        }

        public void OnGhostPieceToggleChange() {
            bool tickOn = ghostPieceToggle.isOn;
            PlayerPrefs.SetInt("drawGhostPiece", tickOn ? 1 : 0);
        }
    }
}
