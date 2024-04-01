using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    [System.Serializable]
    public class ManaIcon
    {
        public Sprite bgSprite;
        public Sprite iconSprite;
        public float xOffset;
        public float yOffset;
        public float xScale = 1f;
        public float yScale = 1f;
        public float rotation;
    }

    [CreateAssetMenu(fileName = "IconPack", menuName = "Cosmetics/IconPack")]
    [System.Serializable]
    public class IconPack : CosmeticItem
    {
        public ManaIcon[] icons;

        public override Color32 iconColor => Color.white;
    }
}