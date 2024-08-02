using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SoloMode {
    [CreateAssetMenu(fileName = "MenuTab", menuName = "ManaCycle/MenuTabs")]
    public class SoloMenuTab : ScriptableObject {
        public string tabName {get; private set;}
        [SerializeField] public LocalizedString tabNameEntry;

        private void OnEnable() {
            tabNameEntry.GetLocalizedStringAsync();
            tabNameEntry.StringChanged += UpdateName;
        }

        private void OnDisable() {
            tabNameEntry.StringChanged -= UpdateName;
        }

        private void UpdateName(string name) {
            tabName = name;
        }

        [SerializeField] public Level[] levelsList;
        [SerializeField] public SoloLevelSeriesProgression progression;
    }
}