using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ManaCycle/Levels")]
public class Level : ScriptableObject {
    [SerializeField] public string levelName = "Level";
    [SerializeField] public string description = "One of the levels of time";
    
    /** Amount of time to complete the level, in seconds. */
    [SerializeField] public double time = 300;
    /** Points needed to complete the level **/
    [SerializeField] public int scoreGoal = 2000;
    /** List of additional objectives that must be met to clear the stage **/
    [SerializeField] public List<Objective> objectives;
}