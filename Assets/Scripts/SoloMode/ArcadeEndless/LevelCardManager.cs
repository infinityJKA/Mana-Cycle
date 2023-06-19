using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// using Sound;

namespace SoloMode
{
    public class LevelCardManager : MonoBehaviour
    {
        [SerializeField] private InputScript inputScript;
        // the level this card represents.
        [SerializeField] public GameObject levelCardPrefab;

        // text at the top of the screen
        [SerializeField] private TextMeshProUGUI matchText; 

        // hp bar and text used to show hp between stages 
        [SerializeField] private Battle.Board.HealthBar hpBar;
        [SerializeField] private TextMeshProUGUI hpText;

        // [SerializeField] private TextMeshProUGUI currencyText;

        // life container and prefab to display lives
        [SerializeField] GameObject lifeContainer;
        [SerializeField] GameObject lifePrefab;

        [SerializeField] TransitionScript transitionHandler;

        void Start()
        {
            if (Storage.level == null) ArcadeStats.inventory = new Dictionary<Item, int>();

            // hide debug cards if not debugging
            if (Storage.level != null)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(false);
            }
            
            // if first match, init player stats and keep hp (determined by previous level) within bounds
            // second null check is for debug
            if ((Storage.level != null && Storage.level.GetBehindCount() == 0) || Storage.level == null)
            {
                ArcadeStats.maxHp = 2000;
                Storage.hp = Mathf.Clamp(Storage.hp, 100, ArcadeStats.maxHp);

                ArcadeStats.inventory = new Dictionary<Item, int>();
                ArcadeStats.equipedItems = new List<Item>();
                ArcadeStats.moneyAmount = 0;
                ArcadeStats.maxEquipSlots = 3;
                ArcadeStats.usedEquipSlots = 0;

                ArcadeStats.playerStats = ArcadeStats.defaultStats;
            }

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

            // update hpbar and hptext
            RefreshInfo();

            // currencyText.text = "" + Storage.arcadeMoneyAmount;

            // update title text
            if (Storage.level != null) matchText.text = "-= Match " + (Storage.level.GetBehindCount() + 1) + " =-";
        }

        void OnEnable()
        {
            RefreshInfo();
        }

        void Update()
        {
            // if (Input.GetKeyDown(inputScript.Cast))
            // {
            //     EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            // }
        }

        public void SelectQuit()
        {
            transitionHandler.WipeToScene("SoloMenu", reverse: true);
        }

        public void RefreshInfo()
        {
            hpText.text = Storage.hp + " / " + ArcadeStats.maxHp + " HP";
            hpBar.Refresh();

            // set life display

            foreach (Transform t in lifeContainer.transform)
            {
                Destroy(t.gameObject);
            }


            for (int i = 0; i < Storage.lives; i++)
            {
                Instantiate(lifePrefab.transform, lifeContainer.transform);
            }
        }
    }
}