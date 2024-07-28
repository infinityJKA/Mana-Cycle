using UnityEngine;
using UnityEngine.UI;

namespace VersusMode 
{
    public class CharSelectCursor : MonoBehaviour 
    {
        [SerializeField] Vector3 offset;
        private RectTransform rt;
        private Vector3 targetPos;
        [SerializeField] float moveTime = 0.1f;
        private Vector3 velocity = Vector3.zero;

        public void SetTarget(Vector3 pos)
        {

            targetPos = pos + offset;
        }

        void Start()
        {
            rt = GetComponent<RectTransform>();
            targetPos = rt.anchoredPosition;
        }

        void Update()
        {
            rt.anchoredPosition = Vector3.SmoothDamp(rt.anchoredPosition, targetPos, ref velocity, moveTime);
        }
    }
}