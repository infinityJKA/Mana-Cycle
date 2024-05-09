using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Fishing/Lure")]
public class FishingLure : FishingItem
{
    public List<FishingEnemy> encounterables = new List<FishingEnemy>();
}
