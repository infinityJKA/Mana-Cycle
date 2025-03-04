using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

using Sound;
using Achievements;
using UnityEngine.Localization.Settings;

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

    [SerializeField] public ScrollRect scrollRect;
    [SerializeField] public float scrollAmount = 0.1f;

    [SerializeField] GameObject puchaseSFX, failPuchaseSFX;

    private int selectionIndex;

    // Start is called before the first frame update
    void Start()
    {
        RefreshText();
    }

    void Awake()
    {
        selectionIndex = 0;
        // add display prefabs for each item in shop inventory
        foreach (Item item in shopItems)
        {
            // Create a new item display & initialize its values
            ItemDisplay itemDisp = Instantiate(itemDisplayPrefab.transform, itemDisplayParent.transform).gameObject.GetComponent<ItemDisplay>();
            itemDisp.item = item;
            itemDisp.showCost = true;
            itemDisp.windowObject = gameObject;

            // add OnSelect functionality, RefreshInfo function
            EventTrigger itemEventTrigger = itemDisp.GetComponent<EventTrigger>();

            // select functionality
            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener(ev => MoveSelection(ev));
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

    public void MoveSelection(BaseEventData ev)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        Item item = selection.GetComponent<ItemDisplay>().item;
        selectionIndex = selection.transform.GetSiblingIndex(); 
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
        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
            if (ArcadeStats.inventory.ContainsKey(item)) ownedText.text = "" + (item.useType != Item.UseType.Equip ? ArcadeStats.inventory[item] : "もう") + "持ってる";
            else ownedText.text = item.useType == Item.UseType.UseOnObtain ? "" : "持ってない";
        }
        else{
            if (ArcadeStats.inventory.ContainsKey(item)) ownedText.text = "" + (item.useType != Item.UseType.Equip ? ArcadeStats.inventory[item] : "Already") + " owned";
            else ownedText.text = item.useType == Item.UseType.UseOnObtain ? "" : "Unowned";
        }
        
    }

    public void BuyItem(BaseEventData ev)
    {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        Item item = selection.GetComponent<ItemDisplay>().item;

        // Debug.Log(item.itemName + " purchase attempt");
        // only buy 1 of each equipable
        bool equipOwnedCheck = (item.useType == Item.UseType.Equip && ArcadeStats.inventory.ContainsKey(item) && ArcadeStats.inventory[item] > 0);

        if (ArcadeStats.moneyAmount >= item.cost && !equipOwnedCheck)
        {
            // buy item
            // Debug.Log("purchase win");

            ArcadeStats.moneyAmount -= item.cost;
            // item.cost = (int) (item.cost * item.costIncreaseMult);
            ArcadeStats.itemCosts[item] = (int) (ArcadeStats.itemCosts[item] * item.costIncreaseMult);
            item.cost = ArcadeStats.itemCosts[item];
            Inventory.ObtainItem(item);
            // update money counters
            RefreshAllDisplays();
            RefreshText();
            selection.GetComponent<ItemDisplay>().Refresh();
            Instantiate(puchaseSFX);

            // activate achivement
            AchievementHandler ah = FindObjectOfType<AchievementHandler>();
            ah.UnlockAchievement("BuyGauntletItem");
            ah.UpdateSteamAchievements();
        }
        else
        {
            // cant buy item
            Debug.Log("purchase fail");
            if (ArcadeStats.moneyAmount < item.cost) moneyDisplays[0].GetComponent<Animation.Shake>().StartShake();
            if (equipOwnedCheck) ownedText.GetComponent<Animation.ColorFlash>().Flash();
            Instantiate(failPuchaseSFX);
        }

    }

    private void RefreshAllDisplays()
    {
        foreach (MoneyDisp m in moneyDisplays)
        {
            m.RefreshText();
        }
    }

    public void OnCloseButtonMove(BaseEventData eventData)
    {
        AxisEventData e = (AxisEventData) eventData;
        if (e.moveDir == MoveDirection.Left) 
        {
            EventSystem.current.SetSelectedGameObject(itemDisplayParent.transform.GetChild(selectionIndex).gameObject);
        }
        if (e.moveDir == MoveDirection.Up)
        {
            EventSystem.current.SetSelectedGameObject(itemDisplayParent.transform.GetChild(0).gameObject);
        }
    }
}
