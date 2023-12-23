using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Animation
{
    public class ColorFader : MonoBehaviour
    {
        [SerializeField] private float fadeTime = 0.75f;

        [SerializeField] private float alphaOverwrite = 1.0f;
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
            Color newColor = Color.Lerp(oldColor, targetColor, fadeTimer / fadeTime);
            img.color = new Color(newColor.r, newColor.g, newColor.b, alphaOverwrite);
            fadeTimer += Time.deltaTime;
        }

        public void FadeToColor(Color newColor)
        {
            if (img == null) return;
            oldColor = img.color;
            targetColor = newColor;
            fadeTimer = 0;
            // gameObject.GetComponent<Image>().color = newColor;
        }
    }

}
