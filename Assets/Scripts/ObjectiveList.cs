using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveList : MonoBehaviour
{
    [SerializeField] List<ObjectiveListItem> objectives;

    public void Refresh(GameBoard board) {
        bool allObjectivesComplete = true;

        foreach (ObjectiveListItem objListItem in objectives) {
            bool completed = objListItem.Refresh(board);
            if (!completed) allObjectivesComplete = false;
        }

        if (allObjectivesComplete) {
            board.Win();
        }
    }
}
