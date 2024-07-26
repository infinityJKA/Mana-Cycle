using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle 
{
    public class BackgroundManager : MonoBehaviour
    {
        [SerializeField] GameObject fallbackBackground;
        [SerializeField] GameObject[] backgrounds;
        // Start is called before the first frame update
        void Start()
        {
            if (PlayerPrefs.GetInt("simpleBG") == 1)
            {
                Instantiate(fallbackBackground);
                return;
            }

            if (Storage.level && Storage.level.backgroundOverride != null) Instantiate(Storage.level.backgroundOverride);
            else Instantiate(backgrounds[Random.Range(0, backgrounds.Length - 1)]);
        }

    }
}

