using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingPlayer : MonoBehaviour
{
    public CharacterController controller;
    public float walkSpeed = 5f;
    public float turningTime = 0.1f;
    public float turnSpeed;

    void Update(){
        // Take in inputs (idk if the new input system will replace this)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f){
            // Calculate and smooth the angle the player will face
            float targetAngle = Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSpeed, turningTime);
            transform.rotation = Quaternion.Euler(0f,angle,0f);

            // Move the player
            controller.Move(direction*walkSpeed*Time.deltaTime);
        }

    }
}
