using System.Collections;
using System.Collections.Generic;
using LootLocker.Requests;
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

        // passed to each item disp
        [SerializeField] private SwapPanelManager panelManager;
        [SerializeField] private PurchaseConfirmationPanel confirmationPanel;
        [SerializeField] private SmoothScrollHandler scroller;

        /// <summary>
        /// List of loaded assets - set in start method
        /// </summary>
        CatalogManager.AssetList<CosmeticItem> assetList;

        /// <summary>
        /// Current index within AssetList that has been displayed up to.
        /// </summary>
        private int assetListIndex;

        void Awake()
        {
            if (shopType == CosmeticItem.CosmeticType.Palette) {
                assetList = CatalogManager.paletteColors;
            } else {
                assetList = CatalogManager.iconPacks;
            }
        }

        private void Start() {
            DestroyDisplays();

            if (!CosmeticShop.instance.useBackendCatalogs) {
                MakeItemsLocalDatabase();
                return;
            }

            // if logged in, run on connected now.
            // if not, RunWhenConnected() will be invoked by PlayerManager upon login complete.
            if (PlayerManager.loggedIn) {
                RunWhenConnected();
            }
        }

        /// <summary>
        /// run when the loot locker session process finishes, SUCCESS OR NOT.
        /// so check if player is actually connected
        /// (called via CosmeticShop.cs)
        /// </summary>
        public void RunWhenConnected() {
            if (!LootLockerSDKManager.CheckInitialized()) {
                Debug.LogWarning("loot locker not connected; using local database for now. (don't include this in release, show error instead)");
                MakeItemsLocalDatabase();
                return;
            }

            // pre-emptively load the first page of this shop tab if there is nothing loaded in this tab.
            if (CatalogManager.paletteColors.lastAfterLoad == -1) {
                CatalogManager.paletteColors.LoadNextPage();
            }
        }

        /// <summary>
        /// Make items for all assets that do not have a display yet, up until the loaded index.
        /// Is called by CosmeticShop.cs via CatalogManager.cs when new shop items are received.
        /// </summary>
        public void MakeItems()
        {
            while (assetListIndex < assetList.assets.Count) {
                // TEMPORARY
                // CreateCosmeticDisplay should eventuall take in a ShopItem instead of a CosmeticItem
                // that way Value & other shop-only info does not need to stored along with cosmetic assets after already purchased
                // but for now cosmeticitemdisplays use cosmeticitems and not shopitems
                assetList.assets[assetListIndex].asset.value = assetList.assets[assetListIndex].cost;

                CreateCosmeticDisplay(assetList.assets[assetListIndex].asset);
                assetListIndex++;
            }
        }

        // Create shop items for the local ScriptableObject based database. for local offline testing only
        void MakeItemsLocalDatabase()
        {
            DestroyDisplays();

            List<CosmeticItem> items = new List<CosmeticItem>();
            switch (shopType)
            {
                case CosmeticItem.CosmeticType.IconPack: items = new List<CosmeticItem>(database.iconPacks); break;
                case CosmeticItem.CosmeticType.Palette: items = new List<CosmeticItem>(database.colors); break;
                default: Debug.Log("Shop type not set!"); break;
            }

            foreach (CosmeticItem item in items)
            {
                CreateCosmeticDisplay(item);
            }
        }

        void CreateCosmeticDisplay(CosmeticItem item) {
            CosmeticItemDisp disp = Instantiate(itemDisplayPrefab, shopItemsContainer.transform).GetComponent<CosmeticItemDisp>();
            disp.item = item;
            disp.descText = descriptionText;
            disp.panelManager = panelManager;
            disp.confirmationPanel = confirmationPanel;
            disp.scroller = scroller;
        }

        void DestroyDisplays() {
            // destroy all children
            for (int i = 0; i < shopItemsContainer.transform.childCount; i++)
            {
                Destroy(shopItemsContainer.transform.GetChild(i).gameObject);
            }
        }
    }
}

