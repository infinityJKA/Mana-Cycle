using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    public abstract class CosmeticItem : ScriptableObject
    {
        public string id;
        // type of cosmetic item this is. prepended to id in database
        private string typeName;

        public string displayName;
        public string description;

        public Sprite icon;
        public Color iconColor = Color.white;

        public bool owned;

        // cost in cosmetic shop
        public int value;

        public string getTypeName()
        {
            return typeName;
        }

        protected CosmeticItem(string typeName = "NONE")
        {
            this.typeName = typeName;
        }
    }
}

