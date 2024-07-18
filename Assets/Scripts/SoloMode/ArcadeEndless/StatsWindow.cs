using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private TextMeshProUGUI valueText;

    [SerializeField] private int leftPad = 30;

    [SerializeField] private GameObject openSFX, closeSFX;

    public void Refresh()
    {
        statText.text = "";
        valueText.text = "";
        foreach (KeyValuePair<ArcadeStats.Stat, float> stat in ArcadeStats.playerStats)
        {
            statText.text += ArcadeStats.StatToString(stat.Key) + "\n";
            for (int i = 0; i < leftPad; i++) valueText.text += " ";
            valueText.text += (stat.Value * (stat.Key == ArcadeStats.Stat.StartingSpecial ? 100 : 1)) + (stat.Key == ArcadeStats.Stat.StartingCycleModifier ? 1 : 0) + ArcadeStats.StatToUnit(stat.Key) + "\n";
        }
    }

    void OnEnable()
    {
        Instantiate(openSFX);
    }

    void OnDisable()
    {
        Instantiate(closeSFX);
    }
}
