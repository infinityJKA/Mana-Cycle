namespace Cosmetics {
    public class ShopItem<T> where T : CosmeticItem {
        public string catalog_listing_id;
        public T item;
        public CurrencyType currencyType;
        public int cost;

        // set in CatalogManager, or MakeLocalDatabase if not using backend catalogs
        public bool owned;
    }

    public enum CurrencyType {
        Coins, // aka ibncoin
        Iridium
    }
}