using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Battle.Board {
    public class HpNum : MonoBehaviour
    {
        public void SetHealth(int health)
        {
            var textbox = GetComponent<TMPro.TextMeshProUGUI>();
            textbox.text = (Math.Max(health, 0)).ToString().PadLeft(4, '0');
        }
    }
}