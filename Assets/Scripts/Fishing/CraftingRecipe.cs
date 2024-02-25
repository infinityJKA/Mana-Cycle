using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CraftingRecipe
{
    public FishingItem itemToCraft;
    public List<FishingItem> requiredItems;
    public List<int> requiredItemCount;

}
