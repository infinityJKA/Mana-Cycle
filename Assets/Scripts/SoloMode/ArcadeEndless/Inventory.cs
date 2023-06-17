using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    // prefab of item display
    [SerializeField] private GameObject itemDisplayPrefab;
    // vert layout for item displays
    [SerializeField] private GameObject itemDisplayParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {
        // destroy old item prefabs
        foreach (Transform t in itemDisplayParent.transform)
        {
            Destroy(t.gameObject);
        }

        // add display prefabs for each item in inventory
        foreach (var kvp in Storage.arcadeInventory)
        {
            Item i = kvp.Key;

            ItemDisplay newDisp = Instantiate(itemDisplayPrefab.transform, itemDisplayParent.transform).gameObject.GetComponent<ItemDisplay>();  
            newDisp.item = i;
            newDisp.showOwnedAmount = true;
            newDisp.windowObject = gameObject;

            // add OnSelect functionality, RefreshInfo function
            EventTrigger eTrigger = newDisp.GetComponent<EventTrigger>();
            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener((data) => {RefreshInfo((BaseEventData)data); });
            eTrigger.triggers.Add(selectEntry);

            // add submit functionality, use item
            EventTrigger.Entry submitEntry = new EventTrigger.Entry();
            submitEntry.eventID = EventTriggerType.Submit;
            submitEntry.callback.AddListener((data) => {SelectItem((BaseEventData)data); });
            eTrigger.triggers.Add(submitEntry);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshInfo(BaseEventData data)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        Item item = selection.GetComponent<ItemDisplay>().item;
    } 

    public void SelectItem(BaseEventData data)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        Item item = selection.GetComponent<ItemDisplay>().item;
    }

    // used by any script that gives player items, like shop or level rewards (soon)
    public static void AddItem(Item item)
    {
        // if the inventory dict already contains an instance of this item, just add to its amount. if not, add to dict
        if (!Storage.arcadeInventory.ContainsKey(item))
        {
            Storage.arcadeInventory.Add(item, 0);
            
        }
        Storage.arcadeInventory[item] += 1;
    }
}
