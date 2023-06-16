using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyDisp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;
    // Start is called before the first frame update
    void Start()
    {
        currencyText.text = "" + Storage.arcadeMoneyAmount;
    }

}
