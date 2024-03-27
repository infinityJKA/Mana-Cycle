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

        [SerializeField] private Material ghostMaterial;
        public void SetVisual(Cosmetics.ManaIcon v, Cosmetics.PaletteColor p)
        {
            // Debug.Log("Setting up visual");
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

        public void SetGhostVisual(Cosmetics.ManaIcon v, Cosmetics.PaletteColor p)
        {
            iconImage.sprite = v.iconSprite;
            bgImage.sprite = v.bgSprite;
            iconImage.color = p.darkColor;
            bgImage.color = p.mainColor;

            iconImage.material = new Material(ghostMaterial);
            iconImage.material.SetColor("_Color", p.mainColor);
            iconImage.material.SetFloat("_Size", 1.2f);

            bgImage.material = new Material(ghostMaterial);
            bgImage.material.SetColor("_Color", p.mainColor);
        }
    }

}