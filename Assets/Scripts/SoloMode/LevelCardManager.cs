using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// using Sound;

namespace SoloMode
{
    public class LevelCardManager : MonoBehaviour
    {
        // the level this card represents.
        [SerializeField] public GameObject levelCardPrefab;

        void Start()
        {
            if (Storage.nextLevelChoices != null)
            {
                foreach (Level level in Storage.nextLevelChoices)
                {
                    // create new card parented by this gameobject (the layout row)
                    GameObject newCard = Instantiate(levelCardPrefab, transform);
                    newCard.GetComponent<LevelCard>().level = level;
                }
            }

            EventSystem.current.SetSelectedGameObject(transform.GetChild(1 + (Storage.nextLevelChoices != null ? 3 : 0) ).gameObject);
        }
    }
}