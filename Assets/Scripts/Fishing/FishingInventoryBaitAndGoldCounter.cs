using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingInventoryBaitAndGoldCounter : MonoBehaviour
{
    private FishingInventory inv;
    [SerializeField] TextMeshProUGUI bait,gold;
    void Start()
    {
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        UpdateText();
    }

    public void UpdateText(){
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        bait.text = inv.bait.ToString();
        gold.text = inv.gold.ToString();
    }

}
