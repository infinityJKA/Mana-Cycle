using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    [CreateAssetMenu(fileName = "Cosmetic Database", menuName = "ManaCycle/Cosmetic Database")]
    public class AchievementDatabase : ScriptableObject
    {
        public List<CosmeticItem> cosmetics;
    }
}