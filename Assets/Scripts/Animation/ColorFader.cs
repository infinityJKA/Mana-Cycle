using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Animation
{
    public class ColorFader : MonoBehaviour
    {
        private static float fadeTime = 1.25f;
        private float fadeTimer;
        private Color oldColor;
        private Color targetColor;

        private Image img;
        // Start is called before the first frame update
        void Start()
        {
            img = gameObject.GetComponent<Image>(); 
            oldColor = Color.white;
            targetColor = Color.white;
        }

        // Update is called once per frame
        void Update()
        {
            img.color = Color.Lerp(oldColor, targetColor, fadeTimer / fadeTime);
            fadeTimer += Time.deltaTime;
        }

        public void updateColor(Color newColor)
        {
            if (img == null) return;
            oldColor = img.color;
            targetColor = newColor;
            fadeTimer = 0;
            // gameObject.GetComponent<Image>().color = newColor;
        }
    }

}
