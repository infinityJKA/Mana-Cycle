using System;
using UnityEngine;
using UnityEngine.UI;

namespace VersusMode {
    ///<summary>Controls the grid of selectable characters in the versus menu.</summary>
    public class VersusTipText : MonoBehaviour {
        ///<summary>Cached list of all children</summary>
        [SerializeField] [InspectorName("Input Script")] private InputScript i;

        void OnValidate() {
            if (!i) {
                Debug.LogError("Missing input script on versus tip text");
                return;
            }
            gameObject.GetComponent<TMPro.TextMeshProUGUI>().text =
            String.Format("{0} - Cursor\n{1} - Select\n{2} - Back",
            Utils.KeySymbol(i.Up)+Utils.KeySymbol(i.Left)+Utils.KeySymbol(i.Down)+Utils.KeySymbol(i.Right),
            Utils.KeySymbol(i.Cast),
            Utils.KeySymbol(i.Pause));
        }
    }
}