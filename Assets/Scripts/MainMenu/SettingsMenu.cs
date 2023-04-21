using UnityEngine;

namespace MainMenu {
    /// <summary>
    /// Controls the settings menu, currently only the close button. (Slider volumes are handled solely by SoundManager)
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private InputScript inputScript;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        
        void Update()
        {
            if (Input.GetKeyDown(inputScript.Pause)) {
                closeButton.onClick.Invoke();
            }
        }
    }
}
