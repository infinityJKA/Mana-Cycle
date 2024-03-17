using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Battle.Board {
    public class HpNum : MonoBehaviour
    {
        TMP_Text label;

        private void Start() {
            label = GetComponent<TMP_Text>();
        }

        public void SetHealth(int health)
        {
            if (!label) label = GetComponent<TMP_Text>();
            label.text = Math.Max(health, 0).ToString().PadLeft(4, '0');
        }
    }
}
