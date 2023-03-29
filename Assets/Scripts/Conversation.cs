using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Conversations", menuName = "ManaCycle/Conversation")]
public class Conversation : ScriptableObject {
    [SerializeField] public string[] dialougeList;
    [SerializeField] public Battler[] speakerOrder;
}