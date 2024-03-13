using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Fishing/Weapon")]
public class FishingWeapon : FishingItem
{
    public int ATK;
    public int DEF;
    public bool healing; // TRUE if healing, FALSE if damaging
    public Element element;
}
