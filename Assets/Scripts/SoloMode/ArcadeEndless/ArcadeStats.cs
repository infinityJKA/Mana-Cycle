using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// pretty much just storage.cs but a different name to keep stuff organized
public class ArcadeStats
{
    // maxhp in arcade endless, could be changed by items in the future
    public static int maxHp = 2000;
    // currency amount in arcade endless
    public static int moneyAmount = 0;
    // player's items in arcade endless. key is item, value is amount owned
    public static Dictionary<Item, int> inventory;


}