using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Shop : MonoBehaviour
{
    // items being sold
    public List<Item> shopItems;
    // prefab of item display
    [SerializeField] private GameObject itemDisplayPrefab;
    // vert layout for item displays
    [SerializeField] private GameObject itemDisplayParent;

    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI typeText;

    // Start is called before the first frame update
    void Start()
    {
        RefreshInfo(null);
    }

    void Awake()
    {
        // add display prefabs for each item in shop inventory
        foreach (Item i in shopItems)
        {
            ItemDisplay newDisp = Instantiate(itemDisplayPrefab.transform, itemDisplayParent.transform).gameObject.GetComponent<ItemDisplay>();
            newDisp.item = i;
            newDisp.showCost = true;
            newDisp.windowObject = gameObject;

            // add OnSelect functionality, RefreshInfo function
            EventTrigger eTrigger = newDisp.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Select;
            entry.callback.AddListener((data) => {RefreshInfo((BaseEventData)data); });
            eTrigger.triggers.Add(entry);
        }

        // set selectOnOpen to first item in shop list
        GetComponent<WindowPannel>().selectOnOpen = (itemDisplayParent.transform.GetChild(0).gameObject);
    }

    public void RefreshInfo(BaseEventData data)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        Item item = selection.GetComponent<ItemDisplay>().item;
        // Debug.Log(data);
        // Debug.Log(this);
        // set item to first in list if not given
        if (item == null) item = itemDisplayParent.transform.GetChild(0).gameObject.GetComponent<ItemDisplay>().item;

        // Debug.Log("is work :)");
        descriptionText.text = item.description;
        typeText.text = item.UseTypeToString();

    }
}
