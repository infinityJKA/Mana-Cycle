using UnityEngine;

public class Shake : MonoBehaviour
{
    /** Central shake position */
    public Vector3 center;
    /** Direction of shake. */
    public Vector3 shakeDirection = Vector3.right;
    /** Starting magnitude of shake. Decreases over duration of shake. */
    public float magnitude = 10f;
    /** Amount of shakes per second */
    public float frequency = 5f;

    /** Length of this shake */
    private float shakeDuration = 0f;
    /** Amount of shake time remaining **/
    private float shakeTime = 0f;
    /** Current magnitude */
    private float currentMagnitude = 0f;

    void Start() {
        center = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        shakeTime -= Time.deltaTime;

        if (shakeTime > 0) {
            currentMagnitude = magnitude * (shakeTime / shakeDuration);
            transform.position = center + shakeDirection * Mathf.Sin(shakeTime * 2*Mathf.PI * frequency) * currentMagnitude;
        } else {
            transform.position = center;
        }
    }

    public void ShakeForDuration(float duration) {
        shakeDuration = duration;
        shakeTime = duration;
    }
}