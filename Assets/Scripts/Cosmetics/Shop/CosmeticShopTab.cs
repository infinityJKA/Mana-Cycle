using System.Collections;
using System.Collections.Generic;
using LootLocker.Requests;
using Mono.CSharp.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

namespace Cosmetics
{
    public class CosmeticShopTab : MonoBehaviour
    {
        
        [SerializeField] private GameObject shopItemsContainer;
        [SerializeField] private GameObject itemDisplayPrefab;

        // public fields accessible by cosmeticItemDIsplays via their tab field (set when instantiated by this class)
        [SerializeField] public TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] public ShopType shopType;
        public enum ShopType {
            PaletteColors,
            IconPacks
        }

        [SerializeField] public SwapPanelManager panelManager;
        [SerializeField] public PurchaseConfirmationPanel confirmationPanel;
        [SerializeField] public SmoothScrollHandler scroller;
        [SerializeField] public Button backButton;

        // used when moving to & from back button
        public Selectable lastSelected {get; set;}

        /// <summary>
        /// List of loaded assets - set in start method
        /// </summary>
        CatalogManager.AssetList<CosmeticItem> assetList;

        /// <summary>
        /// Current index within AssetList that has been displayed up to.
        /// </summary>
        private int assetListIndex;

        // cached on awake
        private SwapPanel swapPanel;

        private bool initialized;

        void Awake()
        {
            if (shopType == ShopType.PaletteColors) {
                assetList = CatalogManager.paletteColors;
            } else {
                assetList = CatalogManager.iconPacks;
            }

            swapPanel = GetComponent<SwapPanel>();
            swapPanel.onOpened.AddListener(() => {
                if (PurchaseConfirmationPanel.instance) PurchaseConfirmationPanel.instance.backToPanel = swapPanel.index;
            });
        }

        private void Start() {
            if (!initialized) Initialize();
        }

        public void Initialize() {
            initialized = true;
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

        public void UpdateDisplays() {
            foreach (CosmeticItemDisp disp in GetComponentsInChildren<CosmeticItemDisp>()) {
                disp.UpdateOwnedOverlay();
            }
        }

        /// <summary>
        /// run when the loot locker session process finishes, SUCCESS OR NOT.
        /// so check if player is actually connected
        /// (called via CosmeticShop.cs)
        /// </summary>
        public void RunWhenConnected() {
            if (!CosmeticShop.instance.useBackendCatalogs) return;

            if (!LootLockerSDKManager.CheckInitialized()) {
                Debug.LogError("using backend catalog, but loot locker not connected!");
                return;
            }

            // pre-emptively load the first page of this shop tab if there is nothing loaded in this tab.
            if (assetList.lastAfterLoad == null && !assetList.loading) {
                assetList.LoadNextPage();
            }
        }

        /// <summary>
        /// Make items for all assets that do not have a display yet, up until the loaded index.
        /// Is called by CosmeticShop.cs via CatalogManager.cs when new shop items are received.
        /// </summary>
        public void MakeItems()
        {
            if (!CosmeticShop.instance.useBackendCatalogs) return;

            assetList.assets = (from e in assetList.assets orderby e.owned select e).ToList();
            while (assetListIndex < assetList.assets.Count) {
                CreateCosmeticDisplay(assetList.assets[assetListIndex]);

                // if first item is loaded while panel active, select it
                if (assetListIndex == 0 && swapPanel.showing) SelectFirstItem();

                assetListIndex++;
            }
        }

        // Create shop items for the local ScriptableObject based database. for local offline testing only
        void MakeItemsLocalDatabase()
        {
            DestroyDisplays();

            List<ShopItem<CosmeticItem>> items = new List<ShopItem<CosmeticItem>>();
            if (shopType == ShopType.IconPacks) {
                foreach (var entry in CosmeticShop.instance.database.iconPacks) {
                    items.Add(new ShopItem<CosmeticItem>()
                    {
                        asset = entry.iconPack,
                        cost = entry.cost
                    });
                }
            } else if (shopType == ShopType.PaletteColors) {
                foreach (var entry in CosmeticShop.instance.database.colors) {
                    items.Add(new ShopItem<CosmeticItem>()
                    {
                        asset = entry.paletteColor,
                        cost = entry.cost
                    });
                }
            } else {
                Debug.LogError("Shop type for "+gameObject+" not set!");
                return;
            }

            foreach (ShopItem<CosmeticItem> shopItem in from e in items orderby e.owned select e)
            {
                CreateCosmeticDisplay(shopItem);
            }

            if (swapPanel.showing) SelectFirstItem();
        }

        void CreateCosmeticDisplay(ShopItem<CosmeticItem> shopItem) {
            CosmeticItemDisp disp = Instantiate(itemDisplayPrefab, shopItemsContainer.transform).GetComponent<CosmeticItemDisp>();
            disp.Init(shopItem, this);

            // if this is the first button loaded and panel is active, select it
            if (swapPanel.showing && shopItemsContainer.transform.childCount == 1) {
                var selectable = disp.GetComponent<Selectable>();
                selectable.Select();
                lastSelected = selectable;
            }
        }

        // Select the first shop item upon open if there is one. (called from onOpened in SwapPanel)
        // If lastSelected is not null that is selected instead.
        public void SelectFirstItem() {
            if (!initialized) Initialize();

            if (lastSelected != null) {
                lastSelected.Select();
                return;
            }

            if (shopItemsContainer.transform.childCount == 0) return;

            var firstChild = shopItemsContainer.transform.GetChild(0);
            if (!firstChild) return;

            var selectable = firstChild.GetComponent<Selectable>();
            if (!selectable) return;

            selectable.Select();
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

