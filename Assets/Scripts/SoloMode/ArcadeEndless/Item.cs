using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 [CreateAssetMenu(fileName = "Item", menuName = "ManaCycle/Item")]
public class Item : ScriptableObject
{  
    /// <summary>
    /// name of item shown in shop and player inventory
    /// </summary>
    public string itemName = "Unamed Item";

    /// <summary>
    /// description of what the item does
    /// </summary>
    public string description = "This item does cool stuff!!!";

    /// <summary>
    /// small image shown in shop and inventory
    /// </summary>
    public Sprite icon;

    /// <summary>
    /// how this item is used when it is interacted with from the inventory 
    /// </summary>
    public UseType useType;

    /// <summary>
    /// what this item does when interacted with
    /// </summary>
    public EffectType effectType;

    /// <summary>
    /// value used by some effects, like hp recovery amount
    /// </summary>
    public float effectValue;

    /// <summary>
    /// cost to purchase in shop
    /// </summary>
    public int cost;

    public enum UseType
    {
        Consume, // lost when used, like a health potion
        Equip, // toggled on/off when used, given a free equip slot
        UseOnObtain, // imediate effect when obtained, for permanent uprades in arcade endless
    }

    public enum EffectType
    {
        IncreaseHpPercent, // increase player hp by percent of max hp (yiik reference)
        IncreaseHpFlat, // increase hp by flat amount
        IncreaseMaxHP, // for equipables / perm upgrades
    }

}
