using UnityEngine;

[CreateAssetMenu(fileName = "Battler", menuName = "ManaCycle/Battler", order = 0)]
public class Battler : ScriptableObject {
    [SerializeField] private string displayName;
    [SerializeField] private Sprite sprite;
    // The exact position & scale that the sprite is drawn on the board
    [SerializeField] private RectTransform boardSpriteTransform;
}