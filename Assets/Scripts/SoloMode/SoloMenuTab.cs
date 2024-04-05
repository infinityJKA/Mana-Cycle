using System.Collections.Generic;
using UnityEngine;

namespace SoloMode {
    [CreateAssetMenu(fileName = "MenuTab", menuName = "ManaCycle/MenuTabs")]
    public class SoloMenuTab : ScriptableObject {
        [SerializeField] public string tabName;
        [SerializeField] public Level[] levelsList;
        [SerializeField] public SoloLevelSeriesProgression progression;
    }
}