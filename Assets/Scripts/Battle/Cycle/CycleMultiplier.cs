using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Cycle {
    public class CycleMultiplier : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI label;

        [SerializeField] private Color[] cycleLevelColors;

        public void Set(int label)
        {
            this.label.text = (1 + (0.2f * label)) + "x";
            this.GetComponent<Image>().color = cycleLevelColors[ Mathf.Min(label, cycleLevelColors.Length-1)];
        }
    }
}
