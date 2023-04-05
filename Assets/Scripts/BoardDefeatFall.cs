// Script that makes the baord go bye bye when lose

using UnityEngine;

public class BoardDefeatFall : MonoBehaviour {
    /** Fall acceleration - gameunits/s^2 (fall speed starts at 0) */
    [SerializeField] float fallAcceleration = 50f;
    /** rotation angular speed acceleration - degrees/s^2 (starts at 0) */
    [SerializeField] float angularAcceleration = 25f;
    /** Initial falling speed */
    [SerializeField] float initialFallSpeed = -60f;

    /** Starting reference position */
    Vector2 startPos;

    float fallDistance, fallSpeed, rotation, angularSpeed;
    bool falling = false;

    void Update() {
        if (falling)
        {
            fallDistance += fallSpeed*Time.unscaledDeltaTime;
            fallSpeed += fallAcceleration*Time.unscaledDeltaTime;
            rotation += angularSpeed*Time.unscaledDeltaTime;
            angularSpeed += angularAcceleration*Time.unscaledDeltaTime;

            transform.localPosition = startPos + Vector2.down*fallDistance;
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }
    }
    
    public void StartFall() {
        if (!falling)
        {
            startPos = transform.localPosition;
            fallDistance = 0;
            fallSpeed = initialFallSpeed;
            rotation = 0;
            angularSpeed = 0;
            falling = true;
        }
    }
}