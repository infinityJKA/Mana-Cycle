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
            current.icons.Clear();
        }

        // ===== DATA
        // key: id
        public readonly Dictionary<string, PaletteColor> paletteColors = new Dictionary<string, PaletteColor>();

        // key: id
        public readonly Dictionary<string, ManaIcon> icons = new Dictionary<string, ManaIcon>();

        // List of icon packs
        // Each string[] contains the mana icon IDs within that pack
        public readonly Dictionary<string, string[]> iconPacks = new Dictionary<string, string[]>();

        // ---- Equipped
        // IDs of equipped colors from paletteColors
        public readonly string[] equippedPaletteColors = new string[5];
        // IDs of equipped icons from manaIcons
        public readonly string[] equippedIcons = new string[5];


        public CosmeticAssets() {
            var defaultAssets = DataManager.instance.defaultAssets;
            foreach (var entry in defaultAssets.colors) {
                AddPaletteColor(entry.paletteColor);
            }
            foreach (var entry in defaultAssets.iconPacks) {
                AddIconPack(entry.iconPack);
            }

            int i = 0;
            foreach (var key in paletteColors.Keys) {
                equippedPaletteColors[i] = key;
                i++;
            }

            i = 0;
            foreach (var key in icons.Keys) {
                equippedIcons[i] = key;
                i++;
            }
        }

        
        public void AddPaletteColor(PaletteColor paletteColor) {
            paletteColors.Add(paletteColor.id, paletteColor);
        }

        public void AddIconPack(IconPack iconPack) {
            string[] manaIconIds = new string[iconPack.icons.Length];

            for (int i = 0; i < iconPack.icons.Length; i++) {
                ManaIcon manaIcon = iconPack.icons[i];
                icons.Add(manaIcon.id, manaIcon);
                manaIconIds[i] = manaIcon.id;
            }

            iconPacks.Add(iconPack.id, manaIconIds);
        }

        public void AddItem(CosmeticItem item) {
            Type t = item.GetType();
            if (t.Equals(typeof(PaletteColor))) {
                AddPaletteColor((PaletteColor)item);
            } else if (t.Equals(typeof(IconPack))) {
                AddIconPack((IconPack)item);
            }
        }
    }
}