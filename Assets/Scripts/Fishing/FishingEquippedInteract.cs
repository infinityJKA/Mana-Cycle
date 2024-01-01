using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;
using System;

public class FishingEquippedInteract : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public GameObject highlight;
    public Image icon;
    public FishingInventory inv;
    
    public FishingItem equippedItem;
    public string itemLocation;
    public FishingItem defaultItem;
    public UIInventoryManager uii;
    public FishingInventoryEquippedStatus FIES;

    void OnEnable(){
        uii = GameObject.Find("UI Inventory Manager").GetComponent<UIInventoryManager>();
        //FIES = transform.parent.GetComponent<FishingInventoryEquippedStatus>();
        Generate();
    }

    public void Generate(){
        // Pulling the data
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        if(itemLocation == "left"){
		    equippedItem = inv.weapon1;
        }
        else if(itemLocation == "right"){
            equippedItem = inv.weapon2;
        }
        else{
            equippedItem = inv.armor1;
        }

        
        icon.sprite = equippedItem.icon;
        
    }

    public void Dequip(){
        if(equippedItem != defaultItem){
            inv.Add(equippedItem);
            if(itemLocation == "left"){
		        inv.weapon1 = defaultItem as FishingWeapon;
            }
            else if(itemLocation == "right"){
                inv.weapon2 = defaultItem as FishingWeapon;
            }
            else{
                inv.armor1 = defaultItem as FishingArmor;
            }
            // Debug.Log("dequipped?");
            // FIES.UpdateDisplay();
            uii.CreateInventoryDisplay();
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData){
        uii.bigImage.sprite = icon.sprite;
        uii.bigName.text = equippedItem.itemName;
        
        int val = equippedItem.sellValue;
        int atk = 0;
        int def = 0;
        string elem = "None";
        string type = "ERROR";
        string desc = equippedItem.inventoryDescription;
        string damageType = "";
        
        if (equippedItem is FishingWeapon){
            type = "=NO WEAPON EQUIPPED=";
            atk = (equippedItem as FishingWeapon).ATK;
            def = (equippedItem as FishingWeapon).DEF;
            if((equippedItem as FishingWeapon).healing){
                damageType = " (HEALING)";
            }
        }
        else{
            type = "=NO ARMOR EQUIPPED=";
            atk = (equippedItem as FishingArmor).ATK;
            def = (equippedItem as FishingArmor).DEF;
            if((equippedItem as FishingArmor).healing){
                damageType = " (HEALING)";
            }
        }

        uii.Description.text =
        "Value: "+ val.ToString()+
        "\nSTR: "+ atk.ToString()+ damageType+
        "\nDEF: "+ def.ToString()+
        "\nElement: "+ elem+
        "\n"+ type
        +"\n\n"+desc;
        
        highlight.SetActive(true);
        //Debug.Log(this.gameObject.name + " was selected");
    }

    public void OnPointerExit(PointerEventData pointerEventData){
        highlight.SetActive(false);
    }
    


}
