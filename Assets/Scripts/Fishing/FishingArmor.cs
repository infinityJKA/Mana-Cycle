using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FishingArmor : FishingItem
{
    public int DEF;
    public int ATK;
    public bool healing;  // TRUE if healing, FALSE if damaging.
    public string element;
}