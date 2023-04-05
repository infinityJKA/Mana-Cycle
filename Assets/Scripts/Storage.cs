using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// just a place to store some temporary values between scenes. 
// bad practice? yes. do we care? not really
public class Storage
{
    // battlers selected by players
    public static Battler battler1;
    public static Battler battler2;
    
    // player/cpu state selected by players
    public static bool isPlayer1;
    public static bool isPlayer2;

    /** level that the player selected */
    public static Level level;

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

}
