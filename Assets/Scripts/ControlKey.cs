using UnityEngine;
using UnityEngine.UI;

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
        // use < > for arrows
        if (code == KeyCode.LeftArrow) {
            textGUI.text = "<";
        } else if (code == KeyCode.RightArrow) {
            textGUI.text = ">";
        } else {
            textGUI.text = code.ToString();
        }
    }
}