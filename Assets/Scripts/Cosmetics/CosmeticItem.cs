using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    public abstract class CosmeticItem : ScriptableObject
    {
        // all these fields should eventually come from lootlocker asset as key/values
        public string id;

        public string displayName;
        public string description;


        public Sprite icon;
        public Color iconColor = Color.white;

        // ==== all fields below probably arent needed once backend shop assets are functional
        public bool owned;

        // type of cosmetic item this is. prepended to id in database
        private CosmeticType cosmeticType;
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

