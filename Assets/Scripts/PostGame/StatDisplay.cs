using TMPro;
using UnityEngine;

public class StatDisplay : MonoBehaviour {
    [SerializeField] private Transform rowTransform;
    [SerializeField] private StatDisplayRow rowPrefab;

    public void Display(MatchStats stats) {
        for (int i = 0; i < rowTransform.childCount; i++)
        {
            Destroy(rowTransform.GetChild(i).gameObject);
        }

        AddRow(Storage.isTwoPlayer ? "Damage Dealt" : "Score", stats.totalScore);
        AddRow("Mana Cleared", stats.totalManaCleared);
        AddRow("Spellcasts", stats.totalSpellcasts);
        AddRow("Pieces Placed", stats.totalPiecesPlaced);
        AddRow("Highest Combo", stats.highestCombo);
        AddRow("Highest Cascade", stats.highestCascade);
        AddRow("Highest Damage", stats.highestSingleDamage);
        if (Storage.isTwoPlayer) AddRow("Damage Countered", stats.totalDamageCountered);

    }

    public void AddRow(string key, string value) {
        StatDisplayRow row = Instantiate(rowPrefab.gameObject, rowTransform).GetComponent<StatDisplayRow>();
        row.Set(key, value);
    }

    public void AddRow(string key, object value) {
        AddRow(key, value.ToString());
    }
}