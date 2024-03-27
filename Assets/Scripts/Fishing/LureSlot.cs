using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;

public class LureSlot : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI stackSizeText;
    public ItemInInventory item;
    public FishingPromptUIManager manager;
    public GameObject cursor;
    public FishingInventory inv;

    public void Start(){
        manager = transform.parent.GetComponent<FishingPromptUIManager>();
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        cursor.SetActive(true);
        //Debug.Log(this.gameObject.name + " was selected");
    }

    public void OnPointerExit(PointerEventData pointerEventData){
        cursor.SetActive(false);
    }

    public void GetDescrption(){
        manager.bigImage.sprite = icon.sprite;
        manager.bigName.text = item.itemData.itemName;
        
        int val = item.itemData.sellValue;
        int atk = 0;
        int def = 0;
        string elem = "N/A";
        string itemType = "ERROR";
        string desc = item.itemData.inventoryDescription;
        string damageType = "";
    
        itemType = "Lure";

        manager.Description.text =
        "Value: "+ val.ToString()+
        "\nSTR: "+ atk.ToString()+ damageType+
        "\nDEF: "+ def.ToString()+
        "\nElement: "+ elem+
        "\nItem Type: "+ itemType
        +"\n\n"+desc;
    }

    public void DrawSlot(){
        if(item == null){Debug.Log("Null item, wtf????");return;}
        
        stackSizeText.text = item.stackSize.ToString();
        icon.sprite = item.itemData.icon;
    
    }

    public void InventoryClick(){
        manager.lureToUse = item.itemData;
        GetDescrption();    
        manager.CreateInventoryDisplay();
    }

}
