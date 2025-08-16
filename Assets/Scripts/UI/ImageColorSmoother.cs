using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class ImageColorSmoother : MonoBehaviour
    {
        private Image image;
        private Color target;
        private Color start;
        private float smoothTime = 0.1f;
        private float refTime = 0;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            image = GetComponent<Image>();
            target = image.color;
            start = image.color;
        }

        void Update()
        {
            image.color = Color.Lerp(start, target, (Time.time - refTime) / smoothTime);
        }

        public void SetAlphaTarget(float alpha, float st = 0.1f)
        {
            SetColorTarget(new Color(image.color.r, image.color.g, image.color.b, alpha), st);
        }

        public void SetColorTarget(Color color, float st = 0.1f)
        {
            target = color;
            smoothTime = st;
            start = image.color;
            refTime = Time.time;
        }
    }
}

