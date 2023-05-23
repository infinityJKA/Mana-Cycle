using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Animation {
    public class Popup : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI valueNum;

        // Length of time the popup remains before fadeout starts
        private static readonly float fadeDelay = 1.25f;

        // How fast the popup fades away
        private static readonly float fadeSpeed = 1f;

        // Time that fadeout will begin
        private float fadeBeginTime;
        // cached canvasgroup
        private CanvasGroup canvasGroup;


        void Start() {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }

        void Update() {
            if (Time.time > fadeBeginTime) {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0, fadeSpeed*Time.smoothDeltaTime);
            }
        }

        // Shows this popup for a brief moment with the given value.
        public void Flash(string value) {
            valueNum.text = value;
            fadeBeginTime = Time.time + fadeDelay;
            canvasGroup.alpha = 1;
        }
    }
}