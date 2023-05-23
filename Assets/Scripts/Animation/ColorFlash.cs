using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Animation {
    public class ColorFlash : MonoBehaviour
    {
        /** Central shake position */
        private Color baseColor = Color.white;

        /** Graphic whose color is being changed by this script. */
        private Graphic graphic;
        /** Color currently being flashed. **/
        public Color flashColor;
        /** Length of this flash */
        private float duration = 0.7f;
        /** Amount of flash time remaining **/
        private float time = 0f;
        
        // Current intensity
        private float intensity;


        void Start() {
            graphic = GetComponent<Graphic>();
            baseColor = graphic.color;
        }

        // Update is called once per frame
        void Update()
        {
            time -= Time.smoothDeltaTime;

            if (time > 0) {
                graphic.color = Color.Lerp(baseColor, flashColor, time / duration);
            } else {
                graphic.color = baseColor;
            }
        }

        public void Flash(Color color, float duration, float intensity) {
            this.intensity = intensity;
            this.flashColor = color;
            this.duration = duration*intensity;
            this.time = duration;
        }

        public void Flash(float intensity) {
            this.intensity = intensity;
            this.time = duration*intensity;
        }

        public void Flash() {
            Flash(1f);
        }

        public void SetBaseColor(Color b)
        {
            this.baseColor = b;
        }
    }
}