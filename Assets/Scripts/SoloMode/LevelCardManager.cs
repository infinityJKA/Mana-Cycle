using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

// using Sound;

namespace SoloMode
{
    public class LevelCardManager : MonoBehaviour
    {
        // the level this card represents.
        [SerializeField] public GameObject levelCardPrefab;

        void Start()
        {
            foreach (Level level in Storage.nextLevelChoices)
            {
                // create new card parented by this gameobject (the layout row)
                GameObject newCard = Instantiate(levelCardPrefab, gameObject.transform);
                newCard.GetComponent<LevelCard>().level = level;
            }
        }
        
    }
}