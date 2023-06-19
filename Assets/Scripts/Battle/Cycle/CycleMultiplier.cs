using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Cycle {
    public class CycleMultiplier : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI label;

        [SerializeField] private Color[] cycleLevelColors;

        [SerializeField] private Board.GameBoard board;

        public void Set(int label)
        {
            this.label.text = (1 + (board.boostPerCycleClear / 10f * label + board.boardStats[ArcadeStats.Stat.StartingCycleModifier])) + "x";
            this.GetComponent<Image>().color = cycleLevelColors[ Mathf.Min(label, cycleLevelColors.Length-1)];
        }
    }
}
