using UnityEngine;
using LootLocker;
using LootLocker.Requests;
using Cosmetics;
using SaveData;

public class InventoryManager {
    private const int paletteColorContext = 231765;
    private const int iconPackContext = 231999;

    public static void GetInventory() {
        LootLockerSDKManager.GetInventory((response) =>
        {
            if (!response.success)
            {
                Debug.LogError("Error getting player inventory: "+response.errorData.message);
                return;
            }

            foreach (var item in response.inventory) {
                if (item.asset == null) continue;

                if (item.asset.context == "Palette Color") {
                    CosmeticAssets.current.AddPaletteColor(AssetToPaletteColor(item.asset));
                }
                else if (item.asset.context == "Icon Pack") {
                    Debug.LogWarning("AssetToIconPack not implemented yet");
                } else {
                    Debug.LogWarning("Unrecognized inventory asset context: "+item.asset.context);
                }
            }
        });
    }

    public static PaletteColor AssetToPaletteColor(LootLockerCommonAsset asset) {
        PaletteColor paletteColor = new PaletteColor
        {
            id = asset.id.ToString(),
            displayName = asset.name,
            description = asset.description
        };
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

        return paletteColor;
    }
}