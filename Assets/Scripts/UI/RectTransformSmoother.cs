using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Menus 
{
    public class RectTransformSmoother : MonoBehaviour
    {
        private RectTransform _rt;
        private RectTransform rt {
            get => (_rt == null) ? GetComponent<RectTransform>() : _rt;
        }

        private Vector3 targetAnchoredPosition;
        private Vector3 targetEulerAngles;
        private Vector3 targetScale;

        private Vector3 posVel = Vector3.zero;
        private Vector3 eaVel = Vector3.zero;
        private Vector3 sVel = Vector3.zero;

        [SerializeField] private float smoothTime = 0.1f;
        // private float refTime;

        // Start is called before the first frame update
        void Start()
        {
            _rt = GetComponent<RectTransform>();

            targetAnchoredPosition = rt.anchoredPosition;
            targetEulerAngles = rt.eulerAngles;
            targetScale = rt.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            rt.anchoredPosition = Vector3.SmoothDamp(rt.anchoredPosition, targetAnchoredPosition, ref posVel, smoothTime);
            rt.eulerAngles = Vector3.SmoothDamp(rt.eulerAngles, targetEulerAngles, ref eaVel, smoothTime);
            rt.localScale = Vector3.SmoothDamp(rt.localScale, targetScale, ref sVel, smoothTime);
        }

        // set target pos and rotation to smoothly transition to
        public void SetTargets(Vector3 pos, Vector3? ea = null, Vector3? s = null, float st = 0.1f)
        {
            if (st == 0f) SetImmediate(pos, ea, s);
            targetAnchoredPosition = pos;
            if (ea.HasValue) targetEulerAngles = ea.Value;
            if (s != null) targetScale = s.Value;
            smoothTime = st;
            // refTime = Time.time;
        }

        // used when script is disabled
        public void SetImmediate(Vector3 pos, Vector3? ea = null, Vector3? s = null) 
        {
            rt.anchoredPosition = pos;
            targetAnchoredPosition = pos;

            if (ea.HasValue)
            {
                rt.eulerAngles = ea.Value;
                targetEulerAngles = ea.Value;
            }

            if (s != null)
            {
                rt.localScale = s.Value;
                rt.localScale = s.Value;
            } 
            
        }

        public void JumpImmediateToTarget()
        {
            SetImmediate(targetAnchoredPosition, targetEulerAngles, targetScale);
        }
    }

}
