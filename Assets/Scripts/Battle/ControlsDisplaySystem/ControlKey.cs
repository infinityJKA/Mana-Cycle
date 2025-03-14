using UnityEngine;
using UnityEngine.UI;

namespace Battle.ControlsDisplaySystem {
    public class ControlKey : MonoBehaviour {
        private KeyCode keyCode;
        [SerializeField] private TMPro.TextMeshProUGUI textGUI;

        // Background image sprite for key
        [SerializeField] private Image keyImage;

        // Sprite when not key pressed
        [SerializeField] private Sprite sprite;
        // Sprite when key pressed
        [SerializeField] private Sprite pressedSprite;

        void Update() {
            if (Input.GetKeyDown(keyCode)) {
                keyImage.sprite = pressedSprite;
            }
            if (Input.GetKeyUp(keyCode)) {
                keyImage.sprite = sprite;
            }
        }

        public void SetKeyCode(KeyCode code) {
            this.keyCode = code;
            textGUI.text = Utils.KeySymbol(code);
        }
    }
}