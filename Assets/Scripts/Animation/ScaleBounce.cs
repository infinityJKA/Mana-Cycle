using UnityEngine;

namespace Animation {
    public class ScaleBounce : MonoBehaviour
    {
        [SerializeField] private AnimationCurve curve;
        private float bounceTimer = -1f;
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private float intensity = 1f;
        private Vector3 startScale;

        void Start()
        {
            startScale = transform.localScale;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (bounceTimer < 0) return;

            float s =  curve.Evaluate(bounceTimer);
            transform.localScale = startScale + new Vector3(s, s) * intensity;

            bounceTimer += Time.deltaTime * timeScale;
            if (bounceTimer >= curve.length)
            {
                bounceTimer = -1f;
                transform.localScale = startScale;
            }
        }

        public void StartBounce() {
            bounceTimer = 0;
        }
    }
}
