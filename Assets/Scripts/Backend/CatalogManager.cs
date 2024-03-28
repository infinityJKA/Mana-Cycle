
// fetches and stores assets from the backend to be displayed in the shop
using System;
using System.Collections.Generic;
using Cosmetics;
using LootLocker;
using LootLocker.Requests;
using UnityEngine;

public class CatalogManager {
    public const int pageSize = 10;

    public static readonly AssetList<CosmeticItem> paletteColors = new PaletteColorList();

    public static readonly AssetList<CosmeticItem> iconPacks = new IconPackList();

    public class PaletteColorList : AssetList<CosmeticItem>
    {
        public override string catalogKey => "palette_colors";

        public override CosmeticItem ConvertAsset(LootLockerCatalogEntry entry, LootLockerAssetDetails details)
        {
            PaletteColor paletteColor = ScriptableObject.CreateInstance<PaletteColor>();

            // TODO: get color data from backend asset info

            return paletteColor;
        }

        public override bool IsOwned(string id)
        {
            return SaveData.Assets.paletteColors.ContainsKey(id);
        }
    }

    public class IconPackList : AssetList<CosmeticItem>
    {
        public override string catalogKey => "icon_packs";

        public override CosmeticItem ConvertAsset(LootLockerCatalogEntry entry, LootLockerAssetDetails details)
        {
            IconPack iconPack = ScriptableObject.CreateInstance<IconPack>();

            // TODO: get icon pack data from backend asset info

            return iconPack;
        }

        public override bool IsOwned(string id)
        {
            return SaveData.Assets.iconPacks.ContainsKey(id);
        }
    }

    /// <summary>
    /// List of locally loaded assets from a catalog.
    /// Contains both an asset list and a prices list.
    /// </summary>
    /// <typeparam name="T">the type of asset</typeparam>
    public abstract class AssetList<T> where T : CosmeticItem {
        public abstract string catalogKey {get;}

        public List<ShopItem<T>> assets = new List<ShopItem<T>>();

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
        /// <param name="entry"></param>
        /// <param name="details"></param>
        /// <returns>the converted item</returns>
        public abstract T ConvertAsset(LootLockerCatalogEntry entry,LootLockerAssetDetails details);

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

                for (int i = 0; i < response.entries.Length; i++) {
                    var entry = response.entries[i];
                    var details = response.asset_details[entry.catalog_listing_id];

                    T convertedItem = ConvertAsset(entry, details);

                    ShopItem<T> shopItem = new ShopItem<T>
                    {
                        asset = convertedItem
                    };

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

                    assets.Add(shopItem);
                }

                if (CosmeticShop.instance) CosmeticShop.instance.UpdateTabs();
            });
        }

        // to be called when shop scene is closed; should free up the memory that the shop items list is taking up.
        public void ClearAllEntries() {
            assets.Clear();
            lastAfterLoad = null;
        }
    }

    public class ShopItem<T> where T : CosmeticItem {
        public T asset;
        public CurrencyType currencyType;
        public int cost;
        public bool owned;

    }

    public enum CurrencyType {
        Coins, // aka ibncoin
        Iridium
    }
}