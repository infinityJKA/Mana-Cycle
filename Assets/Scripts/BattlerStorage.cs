using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerStorage : MonoBehaviour
{
    private static Battler battler1;
    private static Battler battler2;
    private static bool isPlayer1;
    private static bool isPlayer2;

    // i love getters and setters so fucking much
    public void SetBattler1(Battler b)
    {
        battler1 = b;
    }

    public void SetBattler2(Battler b)
    {
        battler2 = b;
    }

    public Battler GetBattler1()
    {
        return battler1;
    }

    public Battler GetBattler2()
    {
        return battler2;
    }

    public void SetPlayerControlled1(bool b)
    {
        isPlayer1 = b;
    }

    public void SetPlayerControlled2(bool b)
    {
        isPlayer2 = b;
    }

    public bool GetPlayerControlled1()
    {
        return isPlayer1;
    }

    public bool GetPlayerControlled2()
    {
        return isPlayer2;
    }

}
