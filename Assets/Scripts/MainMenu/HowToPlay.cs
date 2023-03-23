using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlay : MonoBehaviour
{
    [SerializeField] private string[] HTPTexts;
    private int HTPPage = 0;
    [SerializeField] private TMPro.TextMeshProUGUI currentText;
    [SerializeField] private TMPro.TextMeshProUGUI pageText;

    public void Init()
    {
        HTPPage = 0;
        UpdatePage();
    }

    public void ChangePage(int change)
    {
        HTPPage += change;
        HTPPage = Utils.mod(HTPPage, HTPTexts.Length);
        UpdatePage();
        
    }

    public void UpdatePage()
    {
        currentText.text = HTPTexts[HTPPage];
        pageText.text = "(" + (HTPPage+1) + " / " + HTPTexts.Length + ")";
    }

}
