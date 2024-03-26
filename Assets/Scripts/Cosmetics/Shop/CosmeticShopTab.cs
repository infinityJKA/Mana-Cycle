using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    public class CosmeticShopTab : MonoBehaviour
    {
        [SerializeField] CosmeticDatabase database;
        [SerializeField] GameObject shopItemsContainer;
        [SerializeField] GameObject itemDisplayPrefab;

        [SerializeField] private CosmeticItem.CosmeticType shopType;
        // Start is called before the first frame update
        void Start()
        {
            InitShopContent();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void InitShopContent()
        {
            // destroy all children
            for (int i = 0; i < shopItemsContainer.transform.childCount; i++)
            {
                Destroy(shopItemsContainer.transform.GetChild(i).gameObject);
            }

            List<CosmeticItem> items = new List<CosmeticItem>();
            switch (shopType)
            {
                case CosmeticItem.CosmeticType.IconPack: items = new List<CosmeticItem>(database.iconPacks); break;
                case CosmeticItem.CosmeticType.Palette: items = new List<CosmeticItem>(database.colors); break;
                default: Debug.Log("Shop type not set!"); break;
            }

            foreach(CosmeticItem item in items)
            {
                Instantiate(itemDisplayPrefab, shopItemsContainer.transform);
            }
        }
    }
}

