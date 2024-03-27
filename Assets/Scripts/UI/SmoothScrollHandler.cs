using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothScrollHandler : MonoBehaviour
{
    [SerializeField] public RectTransform scrollTransform;
    private float targetPos = 0.0f;
    [SerializeField] private float speed = 0.25f;
    [SerializeField] private float scrollThreshold = 0.0f;
    private float refTime = 0.0f;
    private float oldPosition = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(targetPos - scrollTransform.anchoredPosition.y) > 0.1) scrollTransform.anchoredPosition = new Vector2(0, Mathf.Lerp(oldPosition, targetPos, (Time.time - refTime) / speed));
    }

    public void setTargetPos(float pos)
    {
        if (pos < scrollThreshold) return;

        targetPos = pos;
        refTime = Time.time;
        oldPosition = scrollTransform.anchoredPosition.y;
    }

}
