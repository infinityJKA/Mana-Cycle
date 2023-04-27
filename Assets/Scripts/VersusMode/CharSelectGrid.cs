using UnityEngine;
using UnityEngine.UI;

namespace VersusMode {
    ///<summary>Controls the grid of selectable characters in the versus menu.</summary>
    public class CharSelectGrid : MonoBehaviour {
        ///<summary>Cached list of all children</summary>
        private CharacterIcon[] icons;

        void OnValidate() {
            icons = transform.GetComponentsInChildren<CharacterIcon>();
        }
    }
}