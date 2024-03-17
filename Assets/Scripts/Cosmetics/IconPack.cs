using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    [System.Serializable]
    public class Icon
    {
        public Sprite sprite;
        public float xOffset;
        public float yOffset;
        public float scale;
        public float rotation;
    }

    [CreateAssetMenu(fileName = "IconPack", menuName = "Cosmetics/IconPack")]
    [System.Serializable]
    public class IconPack : CosmeticItem
    {
        public IconPack() : base("IconPack"){}

        public Icon[] icons;
    }
}