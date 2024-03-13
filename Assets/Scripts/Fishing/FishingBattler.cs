using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingBattler : MonoBehaviour
{
   public string battlerName;
   public int DEF;
   public int RES;
   public int maxHP;
   public int currentHP;
   public FishingWeapon[] attacks;
   public int lastAttackUsed;
}
