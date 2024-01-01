using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI stackSizeText;
    public ItemInInventory item;
    public UIInventoryManager uii;
    public GameObject cursor;
    public FishingInventory inv;

    public void Start(){
        uii = transform.parent.GetComponent<UIInventoryManager>();
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        uii.bigImage.sprite = icon.sprite;
        uii.bigName.text = item.itemData.itemName;
        
        int val = item.itemData.sellValue;
        int atk = 0;
        int def = 0;
        string elem = "None";
        string itemType = "ERROR";
        string desc = item.itemData.inventoryDescription;
        string damageType = "";
        
        if(item.itemData is FishingTome){
            itemType = "Tome";
            desc = "This is a tome that you can read.";
        }
        else if (item.itemData is FishingWeapon){
            itemType = "Weapon";
            atk = (item.itemData as FishingWeapon).ATK;
            def = (item.itemData as FishingWeapon).DEF;
            if((item.itemData as FishingWeapon).healing){
                damageType = " (HEALING)";
            }
        }
        else if (item.itemData is FishingWeapon){
            itemType = "Armor";
            atk = (item.itemData as FishingArmor).ATK;
            def = (item.itemData as FishingArmor).DEF;
            if((item.itemData as FishingArmor).healing){
                damageType = " (HEALING)";
            }
        }
        else if (item.itemData is FishingMaterial){
            itemType = "Material";
        }
        else if (item.itemData is FishingLure){
            itemType = "Lure";
        }

        uii.Description.text =
        "Value: "+ val.ToString()+
        "\nSTR: "+ atk.ToString()+ damageType+
        "\nDEF: "+ def.ToString()+
        "\nElement: "+ elem+
        "\nItem Type: "+ itemType
        +"\n\n"+desc;
        
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

    public void InventoryClick(){
        if(item.itemData is FishingArmor){
            if(inv.armor1 == inv.defaultArmor){
                inv.armor1 = item.itemData as FishingArmor;
                inv.Remove(item.itemData);
            }
            else{
                inv.Add(inv.armor1);
                inv.armor1 = item.itemData as FishingArmor;
                inv.Remove(item.itemData);
            }
            uii.CreateInventoryDisplay();
        }
        else if(item.itemData is FishingWeapon){
            uii.clicked = item.itemData as FishingWeapon;
            uii.WeaponPopup.gameObject.SetActive(true);
        }
    }

}
