using UnityEngine;

[CreateAssetMenu(fileName = "Mid-Level Conversation", menuName = "ManaCycle/MidLevelConversation")]
public class MidLevelConversation : Conversation {
    // Condition where the dialogue will first be shown
    [SerializeField] public AppearCondition appearCondition;
    public enum AppearCondition {
        TimeRemaining,
        PointTotal,
        BlobCount
    }

    // Value, depends on the condition
    [SerializeField] public int value;
}