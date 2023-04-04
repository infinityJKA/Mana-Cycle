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

    // // i love getters and setters so fucking much
    // fuck getters and setters we goin public
    // public void SetBattler1(Battler b)
    // {
    //     battler1 = b;
    // }

    // public void SetBattler2(Battler b)
    // {
    //     battler2 = b;
    // }

    // public Battler GetBattler1()
    // {
    //     return battler1;
    // }

    // public Battler GetBattler2()
    // {
    //     return battler2;
    // }

    // public void SetPlayerControlled1(bool b)
    // {
    //     isPlayer1 = b;
    // }

    // public void SetPlayerControlled2(bool b)
    // {
    //     isPlayer2 = b;
    // }

    // public bool GetPlayerControlled1()
    // {
    //     return isPlayer1;
    // }

    // public bool GetPlayerControlled2()
    // {
    //     return isPlayer2;
    // }

}
