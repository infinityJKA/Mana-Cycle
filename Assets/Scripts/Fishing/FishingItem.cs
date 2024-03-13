using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "Fishing/Item")]
public class FishingItem : ScriptableObject
{
    public string itemName;
    public int buyValue;
    public int sellValue;
    public string inventoryDescription;
    public Sprite icon;
}
