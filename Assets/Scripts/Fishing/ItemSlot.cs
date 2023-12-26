using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI stackSizeText;
    public ItemInInventory item;
    public UIInventoryManager uii;
    public GameObject cursor;

    public void Start(){
        uii = transform.parent.GetComponent<UIInventoryManager>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        uii.bigImage.sprite = icon.sprite;
        uii.bigName.text = item.itemData.itemName;
        if(item.itemData is FishingTome){
            uii.Description.text = 
            "Value: [insert value here later]\nSTR: 000\nDEF: 000\nElement: None\nItem Type: Lore Tome\nThis is a tome that you can read.";
        }
        cursor.SetActive(true);
        //Debug.Log(this.gameObject.name + " was selected");
    }

    public void OnPointerExit(PointerEventData pointerEventData){
        cursor.SetActive(false);
    }

    public void DrawSlot(){
        if(item == null){Debug.Log("Null item, wtf????");return;}
        
        stackSizeText.text = item.stackSize.ToString();
        icon.sprite = item.itemData.icon;
    
    }

}
