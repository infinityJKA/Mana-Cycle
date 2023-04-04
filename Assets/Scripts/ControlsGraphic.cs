using UnityEngine;
using TMPro;

public class ControlsGraphic : MonoBehaviour {
    /** Board to display inputs for */
    [SerializeField] private GameBoard board;

    // References for all key tmpros
    [SerializeField] private ControlKey left, down, right, up, rotateLeft, rotateRight, spellcast;

    void Start() {
        var inputScript = board.soloInputScript != null ? board.soloInputScript : board.inputScript;
        left.SetKeyCode(board.inputScript.Left);
        down.SetKeyCode(board.inputScript.Down);
        right.SetKeyCode(board.inputScript.Right);
        up.SetKeyCode(board.inputScript.Up);
        rotateLeft.SetKeyCode(board.inputScript.RotateLeft);
        rotateRight.SetKeyCode(board.inputScript.RotateRight);
        spellcast.SetKeyCode(board.inputScript.Cast);
    }
}