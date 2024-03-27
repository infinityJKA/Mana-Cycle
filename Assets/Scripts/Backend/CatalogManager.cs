
// fetches and stores assets from the backend to be displayed in the shop
using System;
using System.Collections.Generic;
using Cosmetics;
using LootLocker.Requests;
using UnityEngine;

public class CatalogManager {
    public const int pageSize = 10;

    public static readonly AssetList<PaletteColor> paletteColors = new PaletteColorList();

    public static readonly AssetList<IconPack> iconPacks = new IconPackList();

    public class PaletteColorList : AssetList<PaletteColor>
    {
        public override string catalogKey => "palette_colors";

        public override PaletteColor ConvertAsset(LootLockerCatalogEntry entry, LootLockerAssetDetails details)
        {
            PaletteColor paletteColor = new PaletteColor();

            // TODO: get color data from backend asset info

            return paletteColor;
        }

        public override bool IsOwned(string id)
        {
            return SaveData.Assets.paletteColors.ContainsKey(id);
        }
    }

    public class IconPackList : AssetList<IconPack>
    {
        public override string catalogKey => "palette_colors";

        public override IconPack ConvertAsset(LootLockerCatalogEntry entry, LootLockerAssetDetails details)
        {
            IconPack iconPack = new IconPack();

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
    public abstract class AssetList<T> {
        public abstract string catalogKey {get;}

        public List<ShopItem<T>> assets = new List<ShopItem<T>>();

        // If there are no more items to load beyond the final index
        public bool reachedEnd = false;

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
            string after = assets.Count.ToString();

            LootLockerSDKManager.ListCatalogItems(catalogKey, pageSize, after, (response) =>
            {
                if(!response.success)
                {
                    Debug.Log("error: " + response.errorData.message);
                    Debug.Log("request ID: " + response.errorData.request_id);
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

                    // if items ever get more than one price this code will need to be updated
                    var price = entry.prices[0];
                    if (price.currency_code == "IBN") {
                        shopItem.currencyType = CurrencyType.Coins;
                    } else if (price.currency_code == "IDM") {
                        shopItem.currencyType = CurrencyType.Iridium;
                    } else {
                        Debug.LogWarning(entry + " has unrecognized currency type");
                    }

                    shopItem.cost = price.amount;

                    assets.Add(shopItem);
                }
            });
        }
    }

    public class ShopItem<T> {
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