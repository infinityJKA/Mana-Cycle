using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Objective", menuName = "ManaCycle/Level Objective")]
public class Objective : ScriptableObject {
    public enum ObjectiveType {
        Score,
        ManaCleared
    }

    /** Type of objective this is; defines what quota means for this objective */
    [SerializeField] private ObjectiveType objectiveType;
    /** Quota to reach, depends on the type of objective this is */
    [SerializeField] private int quota;

    public bool IsCompleted(GameBoard board) {
        switch (objectiveType) {
            case ObjectiveType.Score: return board.hp >= quota; // in singleplayer, hp means score
            case ObjectiveType.ManaCleared: return board.getTotalManaCleared() >= quota;
            default: return false;
        }
    }

    public string Status(GameBoard board) {
        switch (objectiveType) {
            case ObjectiveType.Score: return board.getTotalManaCleared()+"/"+quota+" Points";
            case ObjectiveType.ManaCleared: return board.getTotalManaCleared()+"/"+quota+" Mana Cleared";
            default: return "This is an objective";
        }
    }
}