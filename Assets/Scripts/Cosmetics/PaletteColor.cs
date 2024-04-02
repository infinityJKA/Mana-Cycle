using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace Cosmetics
{
    [System.Serializable]
    public class PaletteColor : CosmeticItem
    {
        public Color32 mainColor = Color.white;
        public Color32 darkColor = Color.black;


        public override Color32 iconColor => mainColor;

        public override Sprite icon => CosmeticShop.instance.paletteColorIconSprite;
    }
}