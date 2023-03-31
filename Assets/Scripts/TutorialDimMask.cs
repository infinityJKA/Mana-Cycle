using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDimMask : MonoBehaviour
{
    // Images for all 4 sides around the rect
    [SerializeField] Image left, right, top, bottom, center;

    private static int width = 1920;
    private static int height = 1080;
    
    void Start() {
        Debug.Log(center.rectTransform.anchoredPosition);

        MaskToRect(center.rectTransform);
    }

    public void MaskToRect(RectTransform center) {
        left.rectTransform.sizeDelta = new Vector2(center.anchoredPosition.x, height);

        bottom.rectTransform.anchoredPosition = new Vector2(center.anchoredPosition.x, 0);
        bottom.rectTransform.sizeDelta = new Vector2(center.sizeDelta.x, center.anchoredPosition.y);

        right.rectTransform.anchoredPosition = new Vector2(center.anchoredPosition.x + center.sizeDelta.x, 0);
        right.rectTransform.sizeDelta = new Vector2(width - center.anchoredPosition.x - center.sizeDelta.x, height);

        top.rectTransform.anchoredPosition = new Vector2(center.anchoredPosition.x, center.anchoredPosition.y + center.sizeDelta.y);
        top.rectTransform.sizeDelta = new Vector2(center.sizeDelta.x, height - center.anchoredPosition.y - center.sizeDelta.y);
    }
}
