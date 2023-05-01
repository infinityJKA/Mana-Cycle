using UnityEngine;
using TMPro;

namespace Battle.ControlsDisplaySystem {
    public class ControlsGraphic : MonoBehaviour {
        // References for all key tmpros
        [SerializeField] private ControlKey left, down, right, up, rotateLeft, rotateRight, spellcast;

        public void SetInputs(InputScript inputScript) {
            gameObject.SetActive(true);
            left.SetKeyCode(inputScript.Left);
            down.SetKeyCode(inputScript.Down);
            right.SetKeyCode(inputScript.Right);
            up.SetKeyCode(inputScript.Up);
            rotateLeft.SetKeyCode(inputScript.RotateCCW);
            rotateRight.SetKeyCode(inputScript.RotateCW);
            spellcast.SetKeyCode(inputScript.Cast);
        }
    }
}