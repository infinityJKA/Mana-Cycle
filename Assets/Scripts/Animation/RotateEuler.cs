using UnityEngine;

namespace Animation {
    public class RotateEuler : MonoBehaviour
    {
        public Vector3 eulerSpeed;

        // Update is called once per frame
        void Update()
        {
            transform.eulerAngles = transform.eulerAngles + eulerSpeed*Time.deltaTime;
        }
    }
}