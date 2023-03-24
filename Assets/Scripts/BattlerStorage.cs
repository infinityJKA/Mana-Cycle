using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerStorage : MonoBehaviour
{
    private static Battler battler1;
    private static Battler battler2;

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


}
