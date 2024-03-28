using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothScrollHandler : MonoBehaviour
{
    [SerializeField] public RectTransform contentPanel;
    private ScrollRect scrollRect;
    private Vector2 targetPos;
    [SerializeField] private float elasticity = 0.1f;

    private void Awake() {
        scrollRect = GetComponent<ScrollRect>();
    }

    // Update is called once per frame
    void Update()
    {
        var pos = contentPanel.anchoredPosition;
        contentPanel.anchoredPosition = Vector2.Lerp(pos, targetPos, elasticity);
    }

    public void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        targetPos =
                (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
    }
}
