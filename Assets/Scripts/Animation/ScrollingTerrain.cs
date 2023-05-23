using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingTerrain : MonoBehaviour {
    // Z movement speed
    public float speed;
    // Amount of offset with respect to time
    private static float offset = 1000;

    void Update()
    {
        transform.Translate(speed * Vector3.back * Time.smoothDeltaTime);
        if (transform.position.z > offset) {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - offset*2);
        } else if (transform.position.z < -offset) {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + offset*2);
        }
    }
}