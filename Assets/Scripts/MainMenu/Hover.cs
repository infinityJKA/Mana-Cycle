using UnityEngine;
using System;

namespace MainMenu {
    /** Used to make the question mark in the homepage hover. */
    public class Hover : MonoBehaviour
    {
        private float y;
        private float t;
        private float originY;
        [SerializeField] private float scale;
        [SerializeField] private float speed;
        [SerializeField] private float timeOffset = 0;
        [SerializeField] bool useUnscaledTime = false;
        // Start is called before the first frame update
        void Start()
        {
            t = 0;
            originY = transform.localPosition.y;
        }

        // Update is called once per frame
        void Update()
        {
            t += (useUnscaledTime) ? Time.unscaledDeltaTime : Time.smoothDeltaTime;
            y = (float) Math.Sin(t*speed + timeOffset) * scale;
            transform.localPosition = new Vector3(transform.localPosition.x, originY + y, transform.localPosition.z);
        }
    }
}
