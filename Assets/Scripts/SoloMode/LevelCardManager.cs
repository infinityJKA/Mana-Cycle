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
        [SerializeField] private InputScript inputScript;
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

            // auto select middle card.
            // 3 test cards can be hidden / unhidden from scene for testing. if the scene is not being tested, add 3 to skip those cards in children order.
            EventSystem.current.SetSelectedGameObject(transform.GetChild(1 + (Storage.nextLevelChoices != null ? 3 : 0) ).gameObject);
        }

        void Update()
        {
            if (Input.GetKeyDown(inputScript.Cast))
            {
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            }
        }
    }
}