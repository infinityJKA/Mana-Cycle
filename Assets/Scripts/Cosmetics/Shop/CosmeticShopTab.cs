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

        // passed to each item display to update
        [SerializeField] TMPro.TextMeshProUGUI descriptionText;

        [SerializeField] private CosmeticItem.CosmeticType shopType;

        [SerializeField] private SwapPanelManager panelManager;
        [SerializeField] private PurchaseConfirmationPanel confirmationPanel;
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
                CosmeticItemDisp disp = Instantiate(itemDisplayPrefab, shopItemsContainer.transform).GetComponent<CosmeticItemDisp>();
                disp.item = item;
                disp.descText = descriptionText;
                disp.panelManager = panelManager;
                disp.confirmationPanel = confirmationPanel;
            }
        }
    }
}

