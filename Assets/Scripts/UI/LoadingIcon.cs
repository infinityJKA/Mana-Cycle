using UnityEngine;
using UnityEngine.UI;

public class LoadingIcon : MonoBehaviour {
    [SerializeField] private Image colorImage;

    [SerializeField] private Color[] colors;

    [SerializeField] private AnimationCurve rotationCurve, colorCurve;

    [SerializeField] private float animationDuration = 1f;


    private float t;
    private Color startColor, endColor;
    private float startRotation, endRotation;
    private int colorIndex = 0;

    private void Start() {
        colorImage.color = colors[colorIndex];
        Utils.Shuffle(colors);
    }

    private void Update() {
        t += Time.smoothDeltaTime;
        if (t > animationDuration) {
            t -= animationDuration;

            int oldColorIndex = colorIndex;
            colorIndex += 1;
            if (colorIndex >= colors.Length) colorIndex = 0;

            startColor = colors[oldColorIndex];
            endColor = colors[colorIndex];

            startRotation = endRotation;
            endRotation = startRotation + 90f;
        }

        float rotationT = rotationCurve.Evaluate(t / animationDuration);
        transform.eulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(startRotation, endRotation, rotationT));

        float colorT = colorCurve.Evaluate(t / animationDuration);
        colorImage.color = Color.Lerp(startColor, endColor, Mathf.Clamp01(colorT));
    }
}