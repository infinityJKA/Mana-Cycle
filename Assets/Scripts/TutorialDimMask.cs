using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDimMask : MonoBehaviour
{
    // Images for all 4 sides around the rect
    [SerializeField] Image left, right, top, bottom;

    // RectTransform targets for undimmed area. Index in list is what is used in midlevelconvo mask ID.
    // 0 - full dim
    // 1 - cycle
    // 2 - player board
    [SerializeField] RectTransform[] targets;

    private static int width = 1920;
    private static int height = 1080;
    
    void Start() {
        Hide();
    }

    public void MaskTarget(int id) {
        if (id == -1) {
            Hide();
        } else {
            Show();
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
        left.gameObject.SetActive(true);
        right.gameObject.SetActive(true);
        top.gameObject.SetActive(true);
        bottom.gameObject.SetActive(true);
    }

    public void Hide() {
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
        top.gameObject.SetActive(false);
        bottom.gameObject.SetActive(false);
    }
}
