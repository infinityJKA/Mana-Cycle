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
    [SerializeField] private TextMeshProUGUI ownedText;
    [SerializeField] private GameObject descriptionObject;

    // every object in scene that displays money count
    [SerializeField] List<MoneyDisp> moneyDisplays;

    // Start is called before the first frame update
    void Start()
    {
        RefreshText();
    }

    void Awake()
    {
        // add display prefabs for each item in shop inventory
        foreach (Item i in shopItems)
        {
            // Create a new item display & initialize its values
            ItemDisplay itemDisp = Instantiate(itemDisplayPrefab.transform, itemDisplayParent.transform).gameObject.GetComponent<ItemDisplay>();
            itemDisp.item = i;
            itemDisp.showCost = true;
            itemDisp.windowObject = gameObject;

            // add OnSelect functionality, RefreshInfo function
            EventTrigger itemEventTrigger = itemDisp.GetComponent<EventTrigger>();

            // select functionality
            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener(ev => RefreshSelection(ev));
            itemEventTrigger.triggers.Add(selectEntry);

            // add submit functionality
            EventTrigger.Entry submitEntry = new EventTrigger.Entry();
            submitEntry.eventID = EventTriggerType.Submit;
            submitEntry.callback.AddListener(ev => BuyItem(ev));
            itemEventTrigger.triggers.Add(submitEntry);
        }

        // set selectOnOpen to first item in shop list
        GetComponent<WindowPannel>().selectOnOpen = (itemDisplayParent.transform.GetChild(0).gameObject);
    }

    void Update()
    {
        // debug
        if (Application.isEditor && Input.GetKeyDown(KeyCode.F1))
            {
                ArcadeStats.moneyAmount += 500;
                RefreshAllDisplays();
                // Debug.Log(string.Join(",", ArcadeStats.inventory));
            }
    }

    public void RefreshSelection(BaseEventData ev)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        Item item = selection.GetComponent<ItemDisplay>().item;
        // Debug.Log(data);
        // Debug.Log(this);
        // set item to first in list if not given
        // if (item == null) item = itemDisplayParent.transform.GetChild(0).gameObject.GetComponent<ItemDisplay>().item;

        RefreshText();

    }

    public void RefreshText()
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;

        // if not hovering an item, hide item description box
        if ((selection.GetComponent<ItemDisplay>()) == null)
        {
            descriptionObject.SetActive(false);
            return;
        }
        else descriptionObject.SetActive(true);

        Item item = selection.GetComponent<ItemDisplay>().item;

        descriptionText.text = item.description;
        typeText.text = item.UseTypeToString();
        if (ArcadeStats.inventory.ContainsKey(item)) ownedText.text = "" + ArcadeStats.inventory[item] + " owned";
        else ownedText.text = item.useType == Item.UseType.UseOnObtain ? "" : "Unowned";
    }

    public void BuyItem(BaseEventData ev)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        Item item = selection.GetComponent<ItemDisplay>().item;

        // Debug.Log(item.itemName + " purchase attempt");

        if (ArcadeStats.moneyAmount >= item.cost)
        {
            // buy item
            // Debug.Log("purchase win");

            ArcadeStats.moneyAmount -= item.cost;
            Inventory.ObtainItem(item);
            // update money counters
            RefreshAllDisplays();
            RefreshText();
        }
        else
        {
            // cant buy item
            Debug.Log("purchase fail");
        }

    }

    private void RefreshAllDisplays()
    {
        foreach (MoneyDisp m in moneyDisplays)
        {
            m.RefreshText();
        }
    }
}
