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
        [SerializeField] private CosmeticDatabase database;
        [SerializeField] private GameObject shopItemsContainer;
        [SerializeField] private GameObject itemDisplayPrefab;

        // public fields accessible by cosmeticItemDIsplays via their tab field (set when instantiated by this class)
        [SerializeField] public TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] public CosmeticItem.CosmeticType shopType;
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
            if (shopType == CosmeticItem.CosmeticType.Palette) {
                assetList = CatalogManager.paletteColors;
            } else {
                assetList = CatalogManager.iconPacks;
            }

            swapPanel = GetComponent<SwapPanel>();
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
            assetList.assets = (from e in assetList.assets orderby e.owned select e).ToList();
            while (assetListIndex < assetList.assets.Count) {
                // TEMPORARY
                // CreateCosmeticDisplay should eventuall take in a ShopItem instead of a CosmeticItem
                // that way Value & other shop-only info does not need to stored along with cosmetic assets after already purchased
                // but for now cosmeticitemdisplays use cosmeticitems and not shopitems
                assetList.assets[assetListIndex].asset.value = assetList.assets[assetListIndex].cost;

                CreateCosmeticDisplay(assetList.assets[assetListIndex].asset);

                // if first item is loaded while panel active, select it
                if (assetListIndex == 0 && swapPanel.showing) SelectFirstItem();

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

            foreach (CosmeticItem item in from e in items orderby e.owned select e)
            {
                CreateCosmeticDisplay(item);
            }

            if (swapPanel.showing) SelectFirstItem();
        }

        void CreateCosmeticDisplay(CosmeticItem item) {
            CosmeticItemDisp disp = Instantiate(itemDisplayPrefab, shopItemsContainer.transform).GetComponent<CosmeticItemDisp>();
            disp.Init(item, this);

            // if this is the first button loaded and panel is active, select it
            if (swapPanel.showing && shopItemsContainer.transform.childCount == 1) {
                var selectable = disp.GetComponent<Selectable>();
                selectable.Select();
                lastSelected = selectable;
            }
        }

        // Select the first shop item upon open if there is one. (called from onOpened in SwapPanel)
        public void SelectFirstItem() {
            if (!initialized) Initialize();

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

