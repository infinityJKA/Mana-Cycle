using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    public abstract class CosmeticItem : ScriptableObject
    {
        public string id;
        // type of cosmetic item this is. prepended to id in database
        private CosmeticType cosmeticType;

        public string displayName;
        public string description;

        public Sprite icon;
        public Color iconColor = Color.white;

        public bool owned;

        public enum CosmeticType
        {
            NONE, // used for defaults
            IconPack,
            Palette,
        }

        // cost in cosmetic shop
        public int value;

        public string getTypeName()
        {
            return cosmeticType.ToString();
        }

        protected CosmeticItem(CosmeticType cosmeticType = CosmeticType.NONE)
        {
            this.cosmeticType = cosmeticType;
        }
    }
}

