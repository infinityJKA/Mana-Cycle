using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDimMask : MonoBehaviour
{
    // Images for all 4 sides around the rect
    [SerializeField] Image left, right, top, bottom;

    // RectTransform targets for undimmed area. Index in list is what is used in midlevelconvo mask ID.
    [SerializeField] RectTransform[] targets;

    private static int width = 1920;
    private static int height = 1080;
    
    void Start() {
        MaskTarget(0);
    }

    public void MaskTarget(int id) {
        if (id == -2) {
            Hide();
        } else if (id == -1) {
            Show();
            MaskToRect(new RectTransform());
        } else {
            MaskToRect(targets[id]);
        }
    }

    void MaskToRect(RectTransform center) {
        left.rectTransform.sizeDelta = new Vector2(center.anchoredPosition.x, height);

        bottom.rectTransform.anchoredPosition = new Vector2(center.anchoredPosition.x, 0);
        bottom.rectTransform.sizeDelta = new Vector2(center.sizeDelta.x, center.anchoredPosition.y);

        right.rectTransform.anchoredPosition = new Vector2(center.anchoredPosition.x + center.sizeDelta.x, 0);
        right.rectTransform.sizeDelta = new Vector2(width - center.anchoredPosition.x - center.sizeDelta.x, height);

        top.rectTransform.anchoredPosition = new Vector2(center.anchoredPosition.x, center.anchoredPosition.y + center.sizeDelta.y);
        top.rectTransform.sizeDelta = new Vector2(center.sizeDelta.x, height - center.anchoredPosition.y - center.sizeDelta.y);
    }

    public void Show() {
        left.enabled = true;
        right.enabled = true;
        top.enabled = true;
        bottom.enabled = true;
    }

    public void Hide() {
        left.enabled = false;
        right.enabled = false;
        top.enabled = false;
        bottom.enabled = false;
    }
}
