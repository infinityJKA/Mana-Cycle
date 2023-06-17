using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// just a place to store some temporary values between scenes
public class Storage
{
    // battlers selected by players
    public static Battle.Battler battler1;
    public static Battle.Battler battler2;
    
    // player/cpu state selected by players
    public static bool isPlayerControlled1;
    public static bool isPlayerControlled2;

    // amount of lives players will start with. used for persistence between arcade levels, and also set via versus mode settings
    public static int lives = 3;
    // in arcade mode- persist HP between matches
    public static int hp;
    // maxhp in arcade endless, could be changed by items in the future
    public static int maxHp = 2000;
    // currency amount in arcade endless
    public static int arcadeMoneyAmount = 0;
    // player's items in arcade endless. key is item, value is amount owned
    public static Dictionary<Item, int> arcadeInventory;

    /** level that the player selected */
    public static SoloMode.Level level;

    // if the current battle should use abilities.
    public static bool enableAbilities = true;

    /// <summary>Index of last item the player hovered in the main menu, start there when re-entering menu</summary>
    public static int lastMainMenuItem = 1; // start on solo mode

    /** last index selected in level select list. -1 will select next level that is not cleared, starts off as this */
    public static int lastLevelSelectedIndex = -1;
    public static int lastTabSelectedIndex = -1;

    /** current gamemode selected by player */
    public static GameMode gamemode;
    public enum GameMode {
        Default,
        Solo,
        Versus
    }

    // true when R is pressed to select single player level, so convo doesnt skip first line
    public static bool levelSelectedThisInput;
    // true when R is pressed to prevent input for convo being used in postgamemenu
    public static bool convoEndedThisInput;
    // true when hitting pause during a midlevelconvo, to prevent pausing menu
    public static bool convoSkippedThisInput;

    // used in arcade endless
    public static List<SoloMode.Level> nextLevelChoices;
}
