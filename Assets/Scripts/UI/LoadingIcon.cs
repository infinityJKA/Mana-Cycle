using UnityEngine;
using UnityEngine.UI;

public class LoadingIcon : MonoBehaviour {
    [SerializeField] private Image colorImage;

    [SerializeField] private Color[] colors;

    [SerializeField] private AnimationCurve colorChangeCurve;

    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private float rotationSpeed = 90f;


    private float t;
    private Color startColor, endColor;
    private int colorIndex = 0;

    private void Start() {
        colorImage.color = colors[colorIndex];
    }

    private void Update() {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.smoothDeltaTime);

        t += Time.smoothDeltaTime;
        if (t > animationDuration) {
            t -= animationDuration;

            int oldColorIndex = colorIndex;
            while (colorIndex == oldColorIndex) {
                colorIndex = Random.Range(0, colors.Length);
            }
            startColor = colors[oldColorIndex];
            endColor = colors[colorIndex];
        }

        float animT = t / animationDuration;
        colorImage.color = Color.Lerp(startColor, endColor, colorChangeCurve.Evaluate(animT));
    }
}