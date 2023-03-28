using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingTerrain : MonoBehaviour {
    // Z movement speed
    public float speed;
    // Amount of offset with respect to time
    private float offset;

    void Start() {
        offset = transform.position.z;
    }

    void Update()
    {
        float z = speed*Time.time + offset;
        transform.position = new Vector3(0, 0, z);
    }
}