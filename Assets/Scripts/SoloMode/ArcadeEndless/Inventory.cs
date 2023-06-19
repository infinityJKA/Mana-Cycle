using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Inventory : MonoBehaviour
{
    // prefab of item display
    [SerializeField] private GameObject itemDisplayPrefab;
    // vert layout for item displays
    [SerializeField] private GameObject itemDisplayParent;

    // description and type of hovered item
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI equipText;

    // hp bar and text used to show hp in inv
    [SerializeField] private Battle.Board.HealthBar hpBar;
    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private GameObject descriptionObject;

    void OnEnable()
    {
        BuildItemList();
        RefreshInfo();
    }

    private void BuildItemList()
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        int selectionIndex = selection.transform.GetSiblingIndex(); 

        // destroy old item prefabs
        foreach (Transform t in itemDisplayParent.transform)
        {
            Destroy(t.gameObject);
        }

        // add display prefabs for each item in inventory
        foreach (var kvp in ArcadeStats.inventory)
        {
            Item i = kvp.Key;

            // if 0 of this item is owned, don't show it in inventory
            if (ArcadeStats.inventory[i] <= 0) continue;

            ItemDisplay newDisp = Instantiate(itemDisplayPrefab.transform, itemDisplayParent.transform).gameObject.GetComponent<ItemDisplay>();  
            newDisp.item = i;
            newDisp.showOwnedAmount = true;
            newDisp.windowObject = gameObject;

            // add OnSelect functionality, RefreshInfo function
            EventTrigger eTrigger = newDisp.GetComponent<EventTrigger>();
            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener((data) => {MoveSelection((BaseEventData)data); });
            eTrigger.triggers.Add(selectEntry);

            // add submit functionality, use item
            EventTrigger.Entry submitEntry = new EventTrigger.Entry();
            submitEntry.eventID = EventTriggerType.Submit;
            submitEntry.callback.AddListener((data) => {SelectItem((BaseEventData)data); });
            eTrigger.triggers.Add(submitEntry);
        }

        // we've reset all item display prefabs, so we need to set selection again. use sibling index to keep selection in the same place
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SetSelectedItem(selectionIndex));
    }

    private void RefreshItemList()
    {
        foreach (Transform t in itemDisplayParent.transform)
        {
            ItemDisplay disp = t.gameObject.GetComponent<ItemDisplay>();
            disp.Refresh();
            if (ArcadeStats.inventory[disp.item] <= 0) BuildItemList();
        }
    }

    public void MoveSelection(BaseEventData data)
    {
        // GameObject selection = EventSystem.current.currentSelectedGameObject;
        // Item item = selection.GetComponent<ItemDisplay>().item;

        RefreshInfo();
    } 

    public void RefreshInfo()
    {
        // update hpbar and hptext
        hpText.text = Storage.hp + " / " + ArcadeStats.maxHp + " HP";
        hpBar.Refresh();

        GameObject selection = EventSystem.current.currentSelectedGameObject;
        if (selection == null) return;

        ItemDisplay disp = selection.GetComponent<ItemDisplay>();
        // if not hovering an item, hide item description box
        if (disp == null)
        {
            descriptionObject.SetActive(false);
            return;
        }
        else descriptionObject.SetActive(true);

        Item item = disp.item;

        if (disp != null)
        {
            descriptionText.text = item.description;
            typeText.text = item.UseTypeToString();
        }

        equipText.text = string.Format("Equiped: {0} / {1}", ArcadeStats.usedEquipSlots, ArcadeStats.maxEquipSlots);
        
    }

    public void SelectItem(BaseEventData data)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        int selectionIndex = selection.transform.GetSiblingIndex(); 
        Item item = selection.GetComponent<ItemDisplay>().item;

        // Using item
        switch (item.useType)
        {
            case Item.UseType.Consume:
                item.ActivateEffect(); ArcadeStats.inventory[item] -= 1; break;

            case Item.UseType.Equip: 
                item.ActivateEffect(); break;

            default: 
                item.ActivateEffect(); ArcadeStats.inventory[item] -= 1; break;
        }

        RefreshItemList();
        RefreshInfo();

    }

    private IEnumerator SetSelectedItem(int i)
    {
        yield return new WaitForEndOfFrame();

        // try to select item in the same place as prev. selection if it no longer exists, select one above. if no items, select close button
        try
        {
            EventSystem.current.SetSelectedGameObject(itemDisplayParent.transform.GetChild(i).gameObject);
        }
        catch
        {
            if (itemDisplayParent.transform.childCount < 0) EventSystem.current.SetSelectedGameObject(itemDisplayParent.transform.GetChild(i-1).gameObject);
            else EventSystem.current.SetSelectedGameObject(GetComponent<WindowPannel>().selectOnOpen);
        }
        
        yield return null;
    }

    // used by any script that gives player items, like shop or level rewards (soon)
    public static void AddItem(Item item)
    {
        if (item.useType == Item.UseType.UseOnObtain)
        {
            item.ActivateEffect(); 
            return;
        }
        
        // if the inventory dict already contains an instance of this item, just add to its amount. if not, add to dict
        if (!ArcadeStats.inventory.ContainsKey(item))
        {
            ArcadeStats.inventory.Add(item, 0);
            
        }
        ArcadeStats.inventory[item] += 1;
    }
}
