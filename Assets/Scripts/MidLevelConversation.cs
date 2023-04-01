using UnityEngine;

[CreateAssetMenu(fileName = "Mid-Level Conversation", menuName = "ManaCycle/MidLevelConversation")]
public class MidLevelConversation : Conversation {
    
    [Tooltip("All conditions that need to be met to show this convo")]
    [SerializeField] public Objective[] appearConditions;

    [Tooltip("ID of tutorial mask shown. -1 for full dim, -2 for no dim")]
    [SerializeField] public int tutorialMaskID = -1;

    public bool ShouldAppear(GameBoard board) {
        foreach (Objective condition in appearConditions) {
            if (!condition.IsCompleted(board)) return false;
        }
        return true;
    }
}