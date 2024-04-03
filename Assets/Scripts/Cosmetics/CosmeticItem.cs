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
        [JsonIgnore]
        public string id; // ID comes from json dictionary key

        public string displayName;
        public string description;

        [JsonIgnore]
        public abstract Sprite icon { get; }
        [JsonIgnore]
        public abstract Color32 iconColor {get;}
    }
}

