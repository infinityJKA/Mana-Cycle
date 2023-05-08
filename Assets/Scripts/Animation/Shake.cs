using UnityEngine;

namespace Animation {
    public class Shake : MonoBehaviour
    {
        /** Central shake position */
        public Vector3 center;
        /** Direction of shake. */
        public Vector3 shakeDirection = Vector3.right;
        /** Starting magnitude of shake. Decreases over duration of shake. */
        public float magnitude = 10f;
        /** Amount of shakes per second */
        public float frequency = 5f;
        /** Duration of shake **/
        public float duration = 1f;

        /** Length of this shake */
        private float shakeDuration = 0f;
        /** Amount of shake time remaining **/
        private float shakeTime = 0f;
        /** Current magnitude */
        private float currentMagnitude = 0f;
        
        /** If currently shaking. */
        private bool shaking;
        // Current shake intensity multiplier
        private float intensity;

        // Update is called once per frame
        void Update()
        {
            if (shaking) {
                shakeTime -= Time.deltaTime;

                if (shakeTime > 0) {
                    currentMagnitude = magnitude * (shakeTime / shakeDuration) * intensity;
                    transform.position = center + shakeDirection * Mathf.Sin(shakeTime * 2*Mathf.PI * frequency) * currentMagnitude;
                } else {
                    transform.position = center;
                    shaking = false;
                }
            }
        }

        public void StartShake(float intensity) {
            this.intensity = intensity;
            center = transform.position;
            shakeDuration = duration*intensity;
            shakeTime = duration*intensity;;
            shaking = true;
        }

        public void StartShake() {
            StartShake(1);
        }

        public void StopShake() {
            shaking = false;
        }
    }
}
