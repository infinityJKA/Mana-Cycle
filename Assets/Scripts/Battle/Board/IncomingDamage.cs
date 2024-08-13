using System.Collections;
using System.Collections.Generic;
using Animation;
using UnityEngine;

namespace Battle.Board {
    public class IncomingDamage : MonoBehaviour
    {
        public int dmg { get; private set; }
        public TMPro.TextMeshProUGUI textComponent;

        private ColorFlash colorFlash;

        // Movement speed towards home position of zero vector - screen-widths per second
        public float speed = 0.2f;

        private void Awake() {
            colorFlash = textComponent.GetComponent<ColorFlash>();
        }

        // update - update animation
        void Update() {
            // Animates the text component's position towards this object's position
            textComponent.transform.position = Vector3.MoveTowards(textComponent.transform.position, transform.position, speed * Screen.width * Time.smoothDeltaTime);
        }

        public void SetDamage(int damage)
        {
            dmg = damage;
            if (dmg > 0)
            {
                textComponent.enabled = true;
                textComponent.text = damage.ToString();
            } else {
                textComponent.enabled = false;
            }
        }

        public void AddDamage(int damage)
        {
            SetDamage(dmg + damage);
        }

        public void SubtractDamage(int damage)
        {
            AddDamage(-damage);
        }

        public void PartiallyCounteredAnim() {
            colorFlash.Flash();
        }
    }
}