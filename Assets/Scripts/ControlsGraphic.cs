using UnityEngine;
using TMPro;

public class ControlsGraphic : MonoBehaviour {
    /** Inputs to display */
    [SerializeField] private InputScript inputScript;

    // References for all key tmpros
    [SerializeField] private ControlKey left, down, right, up, rotateLeft, rotateRight, spellcast;

    void Start() {
        left.SetKeyCode(inputScript.Left);
        down.SetKeyCode(inputScript.Down);
        right.SetKeyCode(inputScript.Right);
        up.SetKeyCode(inputScript.Up);
        rotateLeft.SetKeyCode(inputScript.RotateLeft);
        rotateRight.SetKeyCode(inputScript.RotateRight);
        spellcast.SetKeyCode(inputScript.Cast);
    }
}