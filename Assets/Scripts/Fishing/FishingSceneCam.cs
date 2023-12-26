using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingSceneCam : MonoBehaviour
{
    private Vector3 offset;
    public Transform target;
    public float smoothTime;
    private Vector3 currentVelocity = Vector3.zero;
    
    private void Awake(){
        offset = transform.position - target.position;
    }

    private void Update(){
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }

    

}
