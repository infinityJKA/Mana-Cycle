using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation 
{
    public class MoveAndLoop : MonoBehaviour
    {
        [SerializeField] Vector3 speed;
        [SerializeField] Vector3 minPos;
        [SerializeField] Vector3 maxPos;
        [SerializeField] bool useUnscaledTime = false;
        private float signx;
        private float signz;
        // Start is called before the first frame update
        void Start()
        {
            signx = Mathf.Sign(maxPos.x);
            signz = Mathf.Sign(maxPos.z);
        }

        // Update is called once per frame
        void Update()
        {
            transform.localPosition += speed * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            transform.localPosition = new Vector3(
                transform.localPosition.x * signx >= maxPos.x * signx ? minPos.x : transform.localPosition.x, 
                transform.localPosition.y >= maxPos.y ? minPos.y : transform.localPosition.y,
                transform.localPosition.z * signz >= maxPos.z * signz ? minPos.z : transform.localPosition.z
            );
        }
    }
}

