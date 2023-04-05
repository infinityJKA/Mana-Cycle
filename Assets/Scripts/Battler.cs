using System;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Battler", menuName = "ManaCycle/Battler")]
public class Battler : ScriptableObject {
    [SerializeField] public string displayName;

    [SerializeField] public Sprite sprite;

    [SerializeField] public PieceRng pieceRng;

    [SerializeField] public AudioClip voiceSFX;

    // used for the attack popup gradients
    [SerializeField] public Material gradientMat;
}