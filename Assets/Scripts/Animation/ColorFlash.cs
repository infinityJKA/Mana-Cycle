using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        public float duration = 0.7f;
        /** Amount of flash time remaining **/
        private float time = 0f;

        // If set to FlashProperty, animate the "Flash" property on the material instead of the color, from 1 to 0
        [SerializeField] private AnimMode animMode;

        // may be different to produce less intense flashes on same component
        private float intensity = 1f;

        enum AnimMode {
            Graphic,
            FlashProperty,
            TextVertexColor
        }

        // text if in TextVertexColor mode
        private TMP_Text text;


        void Start() {
            graphic = GetComponent<Graphic>();
            baseColor = graphic.color;

            if (animMode == AnimMode.TextVertexColor) {
                text = GetComponent<TMP_Text>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            time -= Time.smoothDeltaTime;

            if (animMode == AnimMode.Graphic) {
                if (time > 0) {
                    graphic.color = Color.Lerp(baseColor, flashColor, time / duration);
                } else {
                    graphic.color = baseColor;
                    enabled = false;
                }
            } else if (animMode == AnimMode.FlashProperty) {
                if (time > 0) {
                    graphic.material.SetFloat("_Flash", Mathf.Lerp(1, 0, time / duration));
                } else {
                    graphic.material.SetFloat("_Flash", 0);
                    enabled = false;
                }
            } else if (animMode == AnimMode.TextVertexColor) {
                if (time > 0) {
                    text.color = Color.Lerp(baseColor, flashColor, time / duration);
                } else {
                    text.color = baseColor;
                    enabled = false;
                }
            }
        }

        public void Flash(Color color, float duration, float intensity) {
            this.flashColor = color;
            this.duration = duration;
            this.intensity = intensity;
            this.time = duration;
            enabled = true;
        }

        public void Flash(float intensity) {
            Flash(flashColor, duration*intensity, intensity);
        }

        public void Flash() {
            Flash(flashColor, duration, 1f);
        }

        public void SetBaseColor(Color b)
        {
            this.baseColor = b;
        }
    }
}