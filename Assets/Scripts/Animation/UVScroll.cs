using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UVScroll : MonoBehaviour
{
    private RawImage img;
    [SerializeField] private Vector2 scrollSpeed;
    private Rect newRect;

    void Start()
    {
        img = GetComponent<RawImage>();
    }

    void Update()
    {
        Rect newRect = new Rect(scrollSpeed.x * Time.time, scrollSpeed.y * Time.time, img.uvRect.width, img.uvRect.height);
        img.uvRect = newRect;
    }
}
