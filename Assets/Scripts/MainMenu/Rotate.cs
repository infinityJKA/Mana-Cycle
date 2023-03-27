using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to make the gears spin
public class Rotate : MonoBehaviour
{
    // axis to rotate around
    public Vector3 axis;
    // Speed in rotations/sec
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axis, speed * 360 * Time.deltaTime);
    }
}
