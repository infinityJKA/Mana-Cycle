using System.Collections.Generic;
using System.IO;
using Cosmetics;
using UnityEngine;

namespace SaveData {
    /// <summary>
    /// Contains a local verison of the player's owned cosmetic items / other owned assets such as future dlc.
    /// when the player owns these, they will be stored locally in the file this class controls.
    /// should be synced with the player's inventory while online and then will maintin bought cosmetics while offline.
    /// </summary>
    [System.Serializable]
    public class Assets {
        public static Assets current {get; set;}

        // Saving & loading
        public const string name = "settings.sav";
        public static readonly string filePath;
        static Assets() {
            filePath = Path.Join(Application.persistentDataPath, name);
            Load();
        }
        public static void Save() {
            FileStorageManager.Save(current, filePath, encrypt: false);
        }
        public static void Load() {
            current = FileStorageManager.Load<Assets>(filePath, decrypt: false);
        }

        public static void ClearSettings() {
            current = new Assets();
        }

        // ===== DATA
        // key: id
        public static readonly Dictionary<string, PaletteColor> paletteColors = new Dictionary<string, PaletteColor>();

        // key: id
        public static readonly Dictionary<string, IconPack> iconPacks = new Dictionary<string, IconPack>();
    }
}