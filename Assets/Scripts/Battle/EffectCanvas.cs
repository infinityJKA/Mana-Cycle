using UnityEngine;

namespace Battle {
    public class EffectCanvas : MonoBehaviour {
        public static EffectCanvas instance;

        private void Awake() {
            instance = this;
        }
    }
}