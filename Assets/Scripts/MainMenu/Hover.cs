using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Hover : MonoBehaviour
{
    private float y;
    private float t;
    private float originY;
    [SerializeField] private float scale;
    [SerializeField] private float speed;
    // Start is called before the first frame update
    void Start()
    {
        t = 0;
        originY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        y = (float) Math.Sin(t*speed) * scale;
        transform.position = new Vector3(transform.position.x, originY + y, transform.position.z);
    }
}
