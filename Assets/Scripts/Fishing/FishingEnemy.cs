using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Enemy", menuName = "Fishing/Enemy")]
public class FishingEnemy : ScriptableObject
{
    public string enemyName;
    public int MaxHP;
    public Sprite battleSprite;
    public Element element;
    public FishingItem[] attacks;
    public FishingItemDrop[] drops;
    public string description;
    public string entryText;
    public string firstHealWitnessedText;
    public string firstAttackWitnessedText;
    public string firstAttackPerformedText;
    public string firstHealPerformedText;
    public string halfHPText;
    public string playerHalfHPText;
    public string playerDefeatText;
    public string deathText;
    public string[] randomTexts;
    public float randomTextPercent;
}

public class FishingItemDrop{
    public FishingItem itemToDrop;
    public float dropRate;
    public int dropRolls;
}
