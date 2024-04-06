using TMPro;
using UnityEngine;

public class StatDisplayRow : MonoBehaviour {
    [SerializeField] private TMP_Text keyLabel, valueLabel;

    public void Set(string key, string value) {
        keyLabel.text = key;
        valueLabel.text = value;
    }
}