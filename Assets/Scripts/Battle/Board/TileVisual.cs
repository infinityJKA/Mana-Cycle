using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battle.Cycle;

namespace Battle.Board {

    [RequireComponent(typeof(Tile))]
    public class TileVisual : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image bgImage;
        public void SetVisual(Cosmetics.ManaIcon v, Cosmetics.ManaPalette p)
        {
            Debug.Log("Setting up visual");
            iconImage.sprite = v.iconSprite;
            bgImage.sprite = v.bgSprite;
            iconImage.color = p.darkColor;
            bgImage.color = p.mainColor;
            iconImage.rectTransform.anchoredPosition = new Vector2(v.xOffset, v.yOffset);
            iconImage.transform.eulerAngles = new Vector3(0f, 0f, v.rotation);
            iconImage.transform.localScale = new Vector3(v.xScale, v.yScale, 1f);
        }

        public void SetObscuredVisual()
        {
            iconImage.color = new Color(0f, 0f, 0f, 0f);
        }
    }

}