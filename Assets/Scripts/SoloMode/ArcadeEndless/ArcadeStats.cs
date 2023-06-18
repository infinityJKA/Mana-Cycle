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

    // the stats of the player, to be modified in AE
    public static Dictionary<Stat, float> playerStats = new Dictionary<Stat, float>();

    // stat defaults, used in all other gamemodes
    // TODO GameBoard doesn't actually read from this yet
    public static Dictionary<Stat, float> defaultStats = new Dictionary<Stat, float>()
    {
        {Stat.Damage_Mult, 1f},
        {Stat.Starting_Special, 0f},
        {Stat.Special_Gain_Mult, 1f},
        {Stat.Starting_Cycle_Mult, 1f},
        {Stat.Cycle_Mult_Increase, 2f},
        {Stat.Quick_Drop_Speed, 2f},

    };

    // types of stats / multipliers to be applied in gameboard scene
    public enum Stat
    {
        Damage_Mult, // damage multiplier 
        Starting_Special, // how much meter you start with as a percent of max meter (0-1f)
        Special_Gain_Mult, // multiplier for how much mana you gain
        Starting_Cycle_Mult, // the cycle multiplier you have at the beginning of the match. normally 1x
        Cycle_Mult_Increase, // how much the multiplier increases each full cycle.
        Quick_Drop_Speed, // the quick drop delay, normally 0.125
    }


}