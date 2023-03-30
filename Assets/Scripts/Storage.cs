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

    /** true if singleplayer, false if versus */
    public static GameMode gamemode;
    public enum GameMode {
        Default,
        Solo,
        Versus
    }

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
