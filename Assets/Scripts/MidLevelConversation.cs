using UnityEngine;

[CreateAssetMenu(fileName = "Mid-Level Conversation", menuName = "ManaCycle/MidLevelConversation")]
public class MidLevelConversation : Conversation {
    /** Condition where the dialogue will first be shown */
    [SerializeField] public AppearCondition appearCondition;
    public enum AppearCondition {
        TimeRemaining,
        PointTotal,
        ManaClearedTotal,
        SpellcastTotal,
        TopCombo,
        BlobCount,
        Defeated, // value irrelevant
        Won // value irrelevant
    }

    /** Value, depends on the condition */
    [SerializeField] public int value;

    /** Method to decide when the conversation should be shown */
    public bool ShouldAppear(GameBoard board)
    {
        switch (appearCondition) {
            case AppearCondition.TimeRemaining: return board.timer.SecondsRemaining() <= value;
            case AppearCondition.PointTotal: return board.hp >= value;
            case AppearCondition.ManaClearedTotal: return board.GetTotalManaCleared() >= value;
            case AppearCondition.SpellcastTotal: return board.GetTotalSpellcasts() >= value;
            case AppearCondition.TopCombo: return board.GetHighestCombo() >= value;
            case AppearCondition.BlobCount: return board.GetBlobCount() >= value;
            case AppearCondition.Defeated: return board.isDefeated();
            case AppearCondition.Won: return board.isWon();
            default: return false;
        }
    }
}