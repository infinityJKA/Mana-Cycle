using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "ManaCycle/Blackjack Card")]
public class BlackjackCard : ScriptableObject
{
    public Sprite cardSprite;
    public int val;
}
