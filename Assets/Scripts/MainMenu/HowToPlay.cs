using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;

public class HowToPlay : MonoBehaviour
{
    [SerializeField] private string[] HTPTexts;
    [SerializeField] private Sprite[] HTPImgs;
    private int HTPPage = 0;
    [SerializeField] private TMPro.TextMeshProUGUI currentText;
    [SerializeField] private Image currentImg;
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
        currentImg.sprite = HTPImgs[HTPPage];
        pageText.text = "(" + (HTPPage+1) + " / " + HTPTexts.Length + ")";
    }

}
