using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoloMode {
    public class ObjectiveListItem : MonoBehaviour
    {
        // Objective this list item represents
        [SerializeField] public Objective objective;
        // The text box contained in this objective
        [SerializeField] public TMPro.TextMeshProUGUI textbox;


        // Refresh this objective's text and color. Additionally, if complete, return true.
        public bool Refresh(Battle.Board.GameBoard board) {
            textbox.text = objective.Status(board);
            bool completed = objective.IsCompleted(board);
            if (completed) textbox.color = (objective.inverted ? Color.red : Color.green);
            return completed;
        }
    }
}