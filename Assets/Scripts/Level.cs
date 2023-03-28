using System;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Levels")]
public class Level : ScriptableObject {
    [SerializeField] public string levelName;
    [SerializeField] public string description;

    [SerializeField] public double minutes;
    [SerializeField] public int scoreGoal;
}