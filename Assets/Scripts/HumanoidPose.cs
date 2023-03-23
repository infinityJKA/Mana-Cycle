using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidPose : MonoBehaviour
{
    Animator animator;
    public int poseNo;

    void Start () {
        animator = GetComponent<Animator>();
        animator.SetInteger("PoseNo", poseNo);
    }



}
