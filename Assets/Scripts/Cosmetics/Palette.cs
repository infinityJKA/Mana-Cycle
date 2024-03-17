using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    [System.Serializable]
    public class ManaPalette
    {
        public Color mainColor = Color.white;
        public Color darkColor = Color.black;
    }

    [CreateAssetMenu(fileName = "Palette", menuName = "Cosmetics/Palette")]
    [System.Serializable]
    public class Palette : CosmeticItem
    {
        public Palette() : base("Palette"){}

        public ManaPalette[] palettes;
    }
}