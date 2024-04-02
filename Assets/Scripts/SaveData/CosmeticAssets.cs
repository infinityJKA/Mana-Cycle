using System;
using System.Collections.Generic;
using System.IO;
using Cosmetics;
using QFSW.QC;
using UnityEngine;

namespace SaveData {
    /// <summary>
    /// Contains a local verison of the player's owned cosmetic items / other owned assets such as future dlc.
    /// when the player owns these, they will be stored locally in the file this class controls.
    /// should be synced with the player's inventory while online and then will maintin bought cosmetics while offline.
    /// </summary>
    [System.Serializable]
    public class CosmeticAssets {
        public static CosmeticAssets current {get; set;}

        // Saving & loading
        public const string name = "cosmetics.json";
        public static readonly string filePath;
        static CosmeticAssets() {
            filePath = Path.Join(Application.persistentDataPath, name);
            Load();
        }
        public static void Save() {
            FileStorageManager.Save(current, filePath, encrypt: false);
        }
        public static void Load() {
            current = FileStorageManager.Load<CosmeticAssets>(filePath, decrypt: false);
        }

        [Command]
        public static void ClearCosmeticAssets() {
            current.paletteColors.Clear();
            current.iconPacks.Clear();
        }

        // ===== DATA
        // key: id
        public readonly Dictionary<string, PaletteColor> paletteColors = new Dictionary<string, PaletteColor>();

        // key: id
        public readonly Dictionary<string, IconPack> iconPacks = new Dictionary<string, IconPack>();

        
        public void AddPaletteColor(PaletteColor paletteColor) {
            paletteColors.Add(paletteColor.id, paletteColor);
        }

        public void AddIconPack(IconPack iconPack) {
            iconPacks.Add(iconPack.id, iconPack);
        }

        public void AddItem(CosmeticItem item) {
            Type t = item.GetType();
            if (t.Equals(typeof(PaletteColor))) {
                paletteColors.Add(item.id, (PaletteColor)item);
            } else if (t.Equals(typeof(IconPack))) {
                iconPacks.Add(item.id, (IconPack)item);
            }
        }
    }
}