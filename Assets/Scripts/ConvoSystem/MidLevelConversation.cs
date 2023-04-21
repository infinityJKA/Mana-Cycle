using UnityEngine;

using SoloMode;

namespace ConvoSystem {
    [CreateAssetMenu(fileName = "Mid-Level Conversation", menuName = "ManaCycle/MidLevelConversation")]
    public class MidLevelConversation : Conversation {
        
        [Tooltip("All conditions that need to be met to show this convo")]
        [SerializeField] public Objective[] appearConditions;

        [Tooltip("ID of tutorial mask shown. 0 for full dim, -1 for no dim")]
        [SerializeField] public int tutorialMaskID = -1;

        public bool ShouldAppear(Battle.Board.GameBoard board) {
            foreach (Objective condition in appearConditions) {
                if (!condition.IsCompleted(board)) return false;
            }
            return true;
        }
    }
}