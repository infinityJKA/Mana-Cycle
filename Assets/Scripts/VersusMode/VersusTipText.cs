using System;
using UnityEngine;
using UnityEngine.UI;

namespace VersusMode {
    ///<summary>Controls the grid of selectable characters in the versus menu.</summary>
    public class VersusTipText : MonoBehaviour {
        ///<summary>Cached list of all children</summary>
        [SerializeField] [InspectorName("Input Script")] private InputScript i;

        public void Refresh() {
            if (!i) {
                Debug.LogError("Missing input script on versus tip text");
                return;
            }
            gameObject.GetComponent<TMPro.TextMeshProUGUI>().text =
            String.Format(
                "{0} - Cursor\n{1} - Select   {2} - Back\n{3} - Info   {4} - Settings",
                Utils.KeySymbols(i.Up, i.Left, i.Down, i.Right),
                Utils.KeySymbol(i.Cast),
                Utils.KeySymbol(i.Pause),
                Utils.KeySymbol(i.RotateCCW),
                Utils.KeySymbol(i.RotateCW)
            );
        }

        void OnValidate() {
            Refresh();
        }
    }
}