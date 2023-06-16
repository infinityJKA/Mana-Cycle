using System.Collections.Generic;
using UnityEngine;

namespace Achievements
{
    [CreateAssetMenu(fileName = "Achievement Database", menuName = "ManaCycle/Achievement Database")]
    public class AchievementDatabase : ScriptableObject
    {
        public List<Achievement> achievements;
    }
}