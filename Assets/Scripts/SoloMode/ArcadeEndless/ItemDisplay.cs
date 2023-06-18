using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class ItemDisplay : MonoBehaviour
{
    // the item this display represents
    [SerializeField] public Item item;

    [SerializeField] private Image displayImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject costDisplayObject;
    [SerializeField] private GameObject amountDisplayObject;
    
    // true in shop, false in inventory
    public bool showCost = false;

    // amount of an item owned, shown in inventory
    public bool showOwnedAmount = false;

    // the gameobject that should have the "main" script of whatever menu this item is shown in.
    [NonSerialized] public GameObject windowObject;

    // Start is called before the first frame update
    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        displayImage.sprite = item.icon;
        nameText.text = item.itemName;
        costText.text = "" + item.cost;
        if (ArcadeStats.inventory.ContainsKey(item)) amountText.text = "x" + ArcadeStats.inventory[item];

        if (showCost) costDisplayObject.SetActive(true);
        if (showOwnedAmount) amountDisplayObject.SetActive(true);
    }

}
