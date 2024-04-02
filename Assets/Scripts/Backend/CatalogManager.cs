
// fetches and stores assets from the backend to be displayed in the shop
using System;
using System.Collections.Generic;
using Cosmetics;
using LootLocker;
using LootLocker.Requests;
using SaveData;
using UnityEngine;

public class CatalogManager {
    public const int pageSize = 10;

    public static readonly AssetList<CosmeticItem> paletteColors = new PaletteColorList();

    public static readonly AssetList<CosmeticItem> iconPacks = new IconPackList();

    public static void PurchaseItem(ShopItem<CosmeticItem> shopItem) {
        var item = shopItem.asset;

        var itemAndQuantity = new LootLockerCatalogItemAndQuantityPair
        {
            catalog_listing_id = shopItem.catalog_listing_id,
            quantity = 1
        };
        LootLockerCatalogItemAndQuantityPair[] items = { itemAndQuantity };

        LootLockerSDKManager.LootLockerPurchaseCatalogItems(WalletManager.walletID, items, (response) =>
        {
            if(!response.success)
            {
                Debug.LogError("Error purchasing shop item: " + response.errorData.message);
                if (!PurchaseConfirmationPanel.instance) {
                    PurchaseConfirmationPanel.instance.PurchaseResponseReceived(success: false);
                }
                return;
            }

            WalletManager.coins -= shopItem.cost;
            CosmeticAssets.current.AddItem(item);
            CosmeticAssets.Save();
            shopItem.owned = true;
            if (CosmeticShop.instance) CosmeticShop.instance.UpdateBalance();
            if (SidebarUI.instance) SidebarUI.instance.UpdateWalletDisplay();

            if (PurchaseConfirmationPanel.instance) {
                PurchaseConfirmationPanel.instance.PurchaseResponseReceived(success: true);
            } else {
                Debug.LogWarning("Purchase success, but item purchased outside of shop scene???");
                return;
            }
        });
    }

    public class PaletteColorList : AssetList<CosmeticItem>
    {
        public override string catalogKey => "palette_colors";

        public override CosmeticItem ConvertAsset(LootLockerCatalogEntry entry, LootLockerAssetDetails details)
        {
            PaletteColor paletteColor = new PaletteColor();

            return paletteColor;
        }

        public override void UpdateItemDetails(CosmeticItem item, LootLockerCommonAsset asset)
        {
            PaletteColor paletteColor = (PaletteColor)item;
            foreach (var kvp in asset.storage) {
                switch(kvp.key) {
                    case "mainColor":
                        if (ColorUtility.TryParseHtmlString(kvp.value, out Color mainColor)) {
                            paletteColor.mainColor = mainColor;
                        }
                        break;
                    case "darkColor":
                        if (ColorUtility.TryParseHtmlString(kvp.value, out Color darkColor)) {
                            paletteColor.darkColor = darkColor;
                        }
                        break;
                }
            }
        }

        public override bool IsOwned(string id)
        {
            return CosmeticAssets.current.paletteColors.ContainsKey(id);
        }

    }

    public class IconPackList : AssetList<CosmeticItem>
    {
        public override string catalogKey => "icon_packs";

        public override CosmeticItem ConvertAsset(LootLockerCatalogEntry entry, LootLockerAssetDetails details)
        {
            IconPack iconPack = new IconPack();
            // todo: add icons according to data received from backend
            return iconPack;
        }

        public override void UpdateItemDetails(CosmeticItem item, LootLockerCommonAsset asset)
        {
            throw new NotImplementedException();
        }

        public override bool IsOwned(string id)
        {
            return CosmeticAssets.current.iconPacks.ContainsKey(id);
        }
    }

    /// <summary>
    /// List of locally loaded assets from a catalog.
    /// Contains both an asset list and a prices list.
    /// </summary>
    /// <typeparam name="T">the type of asset</typeparam>
    public abstract class AssetList<T> where T : CosmeticItem {
        public abstract string catalogKey {get;}

        // The final assets list that is displayed in the shop.
        public List<ShopItem<T>> assets = new List<ShopItem<T>>();
        // shop items are stored here when their catalog data (id, name, price) shows up.
        // shopitmes here may be waiting on data from full asset data API call until 
        // all required data has arrived and item is added to assets list.
        public Dictionary<string, ShopItem<T>> itemsById = new Dictionary<string, ShopItem<T>>();

        // If there are no more items to load beyond the final index
        public bool reachedEnd {get; private set;} = false;

        // the last page that was loaded.
        // null - no page loaded at all
        // "" - first page was loaded, no pagination info used yet though
        // just used as an additional check to not load the same page twice in a row; not that important
        public string lastAfterLoad {get; private set;} = null;

        public bool loading {get; private set;} = false;

        public LootLockerPaginationResponse<string> pagination;

        /// <summary>
        /// Function that should convert the catalog asset listing and details into a native Mana Cycle object representing it.
        /// </summary>
        /// <returns>the converted item</returns>
        public abstract T ConvertAsset(LootLockerCatalogEntry entry,LootLockerAssetDetails details);

        /// <summary>
        /// Call when details arrive about an item.
        /// The asset full details API call is seperate from the catalog assets call, but catalog asset is processed right when received, 
        /// so this just adds additional info, such as the actual palette color for example.
        /// </summary>
        public abstract void UpdateItemDetails(T item, LootLockerCommonAsset asset);

        /// <summary>
        /// Check whether the player already owns this shop item.
        /// </summary>
        public abstract bool IsOwned(string id);

        // load the next {pageSize} assets from this catalog.
        public void LoadNextPage() {

            string after;

            if (pagination != null) {
                after = pagination.next_cursor;
            } else {
                after = null;
            }

            lastAfterLoad = after;

            loading = true;
            LootLockerSDKManager.ListCatalogItems(catalogKey, pageSize, after, (response) =>
            {
                loading = false;
                pagination = response.pagination;
                if(!response.success)
                {
                    Debug.LogError("error loading items from catalog: " + response.errorData.message);
                    lastAfterLoad = null;
                    return;
                }

                if (response.entries.Length == 0) {
                    if (lastAfterLoad == "") {
                        Debug.LogWarning("Catalog "+response.catalog.name+" is empty...");
                    } else {
                        Debug.Log("No more items to load in "+response.catalog.name);
                    }
                    return;
                }

                // store ids to use in the details request later
                // stored in the order that they will added to the shop once asset data arrives
                string[] ids = new string[response.entries.Length];

                for (int i = 0; i < response.entries.Length; i++) {
                    var entry = response.entries[i];
                    var details = response.asset_details[entry.catalog_listing_id];
                    
                    string assetId = details.legacy_id.ToString();

                    if (itemsById.ContainsKey(assetId)) {
                        Debug.LogWarning("Duplicate item loaded: "+details.name);
                        continue;
                    }

                    ids[i] = assetId;

                    if (!entry.purchasable) {
                        Debug.LogWarning("Unpurchasable item in catalog: "+details.name);
                    }

                    T convertedItem = ConvertAsset(entry, details);

                    ShopItem<T> shopItem = new ShopItem<T>
                    {
                        asset = convertedItem,
                        owned = IsOwned(details.id)
                    };
                    
                    itemsById[assetId] = shopItem;
                    
                    convertedItem.id = details.id;
                    convertedItem.displayName = details.name;

                    // if items ever get more than one price this code will need to be updated
                    var price = entry.prices[0];
                    if (price.currency_code == "ibn") {
                        shopItem.currencyType = CurrencyType.Coins;
                    } else if (price.currency_code == "idm") {
                        shopItem.currencyType = CurrencyType.Iridium;
                    } else {
                        Debug.LogWarning(entry + " has unrecognized currency type: "+price.currency_code);
                    }

                    shopItem.cost = price.amount;

                    shopItem.catalog_listing_id = entry.catalog_listing_id;
                }

                // now fetch all details
                LootLockerSDKManager.GetAssetsById(ids, response => {
                    if (!response.success) {
                        Debug.LogError("Error retreiving assets: "+response.errorData.message);
                        return;
                    }

                    // set retrieved info, and add it to the assets list now that full info is in
                    foreach (var assetInfo in response.assets) {
                        var shopItem = itemsById[assetInfo.id.ToString()];
                        UpdateItemDetails(shopItem.asset, assetInfo);
                        assets.Add(shopItem);
                    }

                    // let the cosmetic shop know there is more assets to display that just arrived
                    if (CosmeticShop.instance) CosmeticShop.instance.UpdateTabs();
                });
            });
        }

        // called on login in case of no domain reload
        public void Reset() {
            Debug.Log("reset asset list "+catalogKey);
            assets.Clear();
            itemsById.Clear();
            loading = false;
            lastAfterLoad = null;
            reachedEnd = false;
            pagination = null;
        }

        // to be called when shop scene is closed; should free up the memory that the shop items list is taking up.
        public void ClearAllEntries() {
            assets.Clear();
            lastAfterLoad = null;
        }
    }
}