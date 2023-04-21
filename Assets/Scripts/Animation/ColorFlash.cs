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


        void Start() {
            graphic = GetComponent<Graphic>();
            baseColor = graphic.color;
        }

        // Update is called once per frame
        void Update()
        {
            time -= Time.deltaTime;

            if (time > 0) {
                graphic.color = Color.Lerp(baseColor, flashColor, time / duration);
            } else {
                graphic.color = baseColor;
            }
        }

        public void Flash(Color color, float duration) {
            this.flashColor = color;
            this.duration = duration;
            this.time = duration;
        }

        public void Flash() {
            this.time = duration;
        }
    }
}