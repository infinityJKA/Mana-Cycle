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
    [SerializeField] private GameObject costDisplayObject;
    
    // true in shop, false in inventory
    public bool showCost;

    // the gameobject that should have the "main" script of whatever menu this item is shown in.
    [NonSerialized] public GameObject windowObject;

    // Start is called before the first frame update
    void Start()
    {
        displayImage.sprite = item.icon;
        nameText.text = item.itemName;
        costText.text = "" + item.cost;

        if (!showCost) costDisplayObject.SetActive(false);

        // assigns the OnItemSelect function to when this gameobject's button component is selected
        EventTrigger eTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Select;
        entry.callback.AddListener((data) => {OnItemSelect((BaseEventData)data); });
        eTrigger.triggers.Add(entry);
    }

    // when the item is hovered in the menu, not when it is pressed
    public void OnItemSelect(BaseEventData data)
    {
        // i feel like theres probably a better way to implement something like this but i cant think of it atm
        if (windowObject.GetComponent<Shop>() != null)
        {
            windowObject.GetComponent<Shop>().RefreshInfo(item);
        }
        // else if (windowObject.GetComponent<Inventory>() != null)
        // {
        //     windowObject.GetComponent<Inventory>().RefreshInfo();
        // }

    }

}