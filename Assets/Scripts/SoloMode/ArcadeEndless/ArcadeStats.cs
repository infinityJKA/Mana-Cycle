using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of the gauntlet mode game state
public class ArcadeStats
{
    // maxhp in arcade endless, changed by some items
    public static int maxHp = 2000;
    // currency amount in arcade endless
    public static int moneyAmount = 0;
    // player's items in arcade endless. key is item, value is amount owned
    public static Dictionary<Item, int> inventory;

    // items the player has equiped. items are not removed from inventory when equip
    public static List<Item> equipedItems = new List<Item>();

    // max equipables you can have on at once. can be increased during gameplay
    public static int maxEquipSlots = 3;

    public static int usedEquipSlots = 0;

    // set by levelCardManager
    public static List<Item> itemRewardPool;

    // stat defaults, used in all other gamemodes
    public static Dictionary<Stat, float> defaultStats = new Dictionary<Stat, float>()
    {
        {Stat.DamageMult, 1f},
        {Stat.StartingSpecial, 0f},
        {Stat.SpecialGainMult, 1f},
        {Stat.StartingCycleModifier, 0f}, 
        {Stat.CycleMultIncrease, 0.2f},
        {Stat.QuickDropSpeed, 0.125f},
        {Stat.MoneyMult, 1f},

    };

    // the stats of the player, to be modified in AE
    public static Dictionary<Stat, float> playerStats = new Dictionary<Stat, float>(defaultStats);

    // prices of shop items during run, may not be base cost
    public static Dictionary<Item, int> itemCosts = new Dictionary<Item, int>();

    // the stats to change at the end of a match. also reset after each match. used for temp mid-game stat boosts
    public static Dictionary<Stat, float> deltaPlayerStats = new Dictionary<Stat, float>()
        {
        {Stat.DamageMult, 0f},
        {Stat.StartingSpecial, 0f},
        {Stat.SpecialGainMult, 0f},
        {Stat.StartingCycleModifier, 0f}, 
        {Stat.CycleMultIncrease, 0f},
        {Stat.QuickDropSpeed, 0f},
        {Stat.MoneyMult, 0f},

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
        MoneyMult, // multiplier for money gained in AE. starts at 1x
    }

    public static string StatToString(Stat s)
    {
        switch(s)
        {
            case Stat.DamageMult: return "Damage Multiplier";
            case Stat.StartingSpecial: return "Starting Special";
            case Stat.SpecialGainMult: return "Special Gain Multiplier";
            case Stat.StartingCycleModifier: return "Starting Cycle Modifier";
            case Stat.CycleMultIncrease: return "Full Cycle Bonus";
            case Stat.QuickDropSpeed: return "Quick Drop Speed";
            case Stat.MoneyMult: return "Money Multiplier";
            default: return "Unamed Stat";
        }
    }

    public static string StatToUnit(Stat s)
    {
        switch(s)
        {
            case Stat.StartingSpecial: return "%";
            case Stat.QuickDropSpeed: return "";
            default: return "x";
        }
    }

}