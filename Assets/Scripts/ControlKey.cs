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
        keyCode = code;
        textGUI.text = code.ToString();

        // bandaid fix for better position of down arrow
        if (keyCode == KeyCode.DownArrow) {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(80, rectTransform.anchoredPosition.y);
        }
    }
}