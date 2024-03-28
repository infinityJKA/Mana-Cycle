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
    [SerializeField] private bool horizontal = false, vertical = true;

    private void Awake() {
        scrollRect = GetComponent<ScrollRect>();
    }

    // Update is called once per frame
    void Update()
    {
        var pos = contentPanel.anchoredPosition;
        var nextPos = Vector2.Lerp(pos, targetPos, elasticity * 100 * Time.smoothDeltaTime);
        if (!horizontal) nextPos.x = pos.x;
        if (!vertical) nextPos.y = pos.y;
        contentPanel.anchoredPosition = nextPos;
    }

    public void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        targetPos =
                (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
    }
}
