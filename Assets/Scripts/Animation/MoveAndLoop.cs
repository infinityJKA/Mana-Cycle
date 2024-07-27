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
        // Start is called before the first frame update
        void Start()
        {
            signx = Mathf.Sign(maxPos.x);
        }

        // Update is called once per frame
        void Update()
        {
            transform.position += speed * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            transform.position = new Vector3(
                transform.position.x * signx >= maxPos.x * signx ? minPos.x : transform.position.x, 
                transform.position.y >= maxPos.y ? minPos.y : transform.position.y,
                transform.position.z >= maxPos.z ? minPos.z : transform.position.z
            );
        }
    }
}

