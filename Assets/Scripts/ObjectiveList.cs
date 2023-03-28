using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveList : MonoBehaviour
{
    [SerializeField] List<ObjectiveListItem> objectives;

    public void Refresh(GameBoard board) {
        bool allObjectivesComplete = true;

        foreach (ObjectiveListItem objective in objectives) {
            objective.Refresh(board);
            if (objective.IsCompleted(board)) {
                objective.textbox.color = Color.green;
            } else {
                allObjectivesComplete = false;
            }
        }

        if (allObjectivesComplete) {
            board.Win();
        }
    }
}
