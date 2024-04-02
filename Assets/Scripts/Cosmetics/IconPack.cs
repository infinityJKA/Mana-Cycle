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

        public Vector2 offset;
        public float xOffset => offset.x;
        public float yOffset => offset.y;

        public Vector2 scale;
        public float xScale => scale.x;
        public float yScale => scale.y;
        
        public float rotation;
    }

    [System.Serializable]
    public class IconPack : CosmeticItem
    {
        public ManaIcon[] icons;

        public override Color32 iconColor => Color.white;

        public override Sprite icon => throw new System.NotImplementedException();
    }
}