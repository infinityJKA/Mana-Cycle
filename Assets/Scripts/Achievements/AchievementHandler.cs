using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Achievements 
{
    public class AchievementHandler : MonoBehaviour 
    {
        public static AchievementHandler Instance;
        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else{
                Destroy(gameObject);
            }
        }
    }
}