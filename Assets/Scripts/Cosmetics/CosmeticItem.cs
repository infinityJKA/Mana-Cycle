using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmetics
{
    [System.Serializable]
    public abstract class CosmeticItem
    {
        // all these fields should eventually come from lootlocker asset as key/values
        public string id;

        public string displayName;
        public string description;

        public abstract Sprite icon { get; }
        public abstract Color32 iconColor {get;}
    }
}

