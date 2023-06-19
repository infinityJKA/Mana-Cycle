using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// pretty much just storage.cs but a different name to keep stuff organized
public class ArcadeStats
{
    // maxhp in arcade endless, changed by some items
    public static int maxHp = 2000;
    // currency amount in arcade endless
    public static int moneyAmount = 0;
    // player's items in arcade endless. key is item, value is amount owned
    public static Dictionary<Item, int> inventory;

    // items the player has equiped. items are not removed from inventory when equip
    public static List<Item> equipedItems;

    // max equipables you can have on at once. can be increased during gameplay
    public static int maxEquipSlots = 3;

    public static int usedEquipSlots = 0;

    // the stats of the player, to be modified in AE
    public static Dictionary<Stat, float> playerStats = new Dictionary<Stat, float>();

    // stat defaults, used in all other gamemodes
    public static Dictionary<Stat, float> defaultStats = new Dictionary<Stat, float>()
    {
        {Stat.DamageMult, 1f},
        {Stat.StartingSpecial, 0f},
        {Stat.SpecialGainMult, 1f},
        {Stat.StartingCycleModifier, 0f}, 
        {Stat.CycleMultIncrease, 0.2f},
        {Stat.QuickDropSpeed, 0.125f}, // not implemented yet

    };

    // types of stats / multipliers to be applied in gameboard scene
    // more types to be added if item concepts need them
    public enum Stat
    {
        DamageMult, // damage multiplier 
        StartingSpecial, // how much meter you start with as a percent of max meter (0-1f)
        SpecialGainMult, // multiplier for how much mana you gain
        StartingCycleModifier, // extra cycle multiplier you have at the beginning of the match. normally 0x, can be negative
        CycleMultIncrease, // how much the multiplier increases each full cycle. normally 0.2
        QuickDropSpeed, // the quick drop delay, normally 0.125
    }


}