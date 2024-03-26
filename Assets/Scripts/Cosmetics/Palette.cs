using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Cosmetics
{
    [CreateAssetMenu(fileName = "Palette", menuName = "Cosmetics/Palette")]
    [System.Serializable]
    public class ManaPalette : CosmeticItem
    {
        public ManaPalette() : base(CosmeticType.Palette){}
        public Color mainColor = Color.white;
        public Color darkColor = Color.black;

        public override bool Equals(object obj)
        {
            return Equals(obj as ManaPalette);
        }

        public bool Equals(ManaPalette other)
        {
            if (other == null) return false;
            return (mainColor == other.mainColor && darkColor == other.darkColor);
        }
    }

    // [CreateAssetMenu(fileName = "Palette", menuName = "Cosmetics/Palette")]
    // [System.Serializable]
    // public class Palette : CosmeticItem
    // {
    //     public Palette() : base("Palette"){}

    //     public ManaPalette[] palettes;
    // }
}