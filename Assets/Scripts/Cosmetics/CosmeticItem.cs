using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public abstract Sprite icon { get; }
        [JsonIgnore]
        public abstract Color32 iconColor {get;}
    }
}

