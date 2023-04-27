using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

namespace VersusMode {

    [ExecuteAlways]
    public class CharacterIcon : MonoBehaviour {
        [Tooltip("The battler that is displayed on this icon")]
        [SerializeField] private Battle.Battler battler;

        [SerializeField] private Image background;
        [SerializeField] private Image portrait;

        private void OnValidate() {
            background.material = battler.gradientMat;
            portrait.sprite = battler.sprite;
        }
    }
}