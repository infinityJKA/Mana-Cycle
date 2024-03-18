using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    [CreateAssetMenu(fileName = "Cosmetic Database", menuName = "ManaCycle/Cosmetic Database")]
    public class CosmeticDatabase : ScriptableObject
    {
        public List<IconPack> icons;
        public List<Palette> palettes;
    }
}