using SoloMode;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace Achievements
{
    [CreateAssetMenu(fileName = "Achievement Database", menuName = "ManaCycle/Achievement Database")]
    public class AchievementDatabase : ScriptableObject
    {
        public List<Achievement> achievements;
    }

    #if (UNITY_EDITOR)
    [CustomEditor(typeof(AchievementDatabase))]
    public class AchievementDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Reset Achievement Progress"))
            {
                AchievementDatabase database = (AchievementDatabase)target;
                
                foreach (var ach in database.achievements)
                {
                    FBPP.DeleteKey("ACH_" + ach.id);  
                }

                FBPP.Save();
                Debug.Log("Achievement progress reset");
            }
        }
    }
    #endif
}