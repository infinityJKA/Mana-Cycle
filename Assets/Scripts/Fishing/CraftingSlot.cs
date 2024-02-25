using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;

public class CraftingSlot : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI craftableText;
    public CraftingRecipe recipe;
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
        uii.bigName.text = recipe.itemToCraft.itemName;
        
        int val = recipe.itemToCraft.sellValue;
        int atk = 0;
        int def = 0;
        string elem = "N/A";
        string itemType = "ERROR";
        string desc = recipe.itemToCraft.inventoryDescription;
        string damageType = "";
        
        if(recipe.itemToCraft is FishingTome){
            itemType = "Tome";
            desc = "This is a tome that you can read.";
        }
        else if (recipe.itemToCraft is FishingWeapon){
            itemType = "Weapon";
            atk = (recipe.itemToCraft as FishingWeapon).ATK;
            def = (recipe.itemToCraft as FishingWeapon).DEF;
            if((recipe.itemToCraft as FishingWeapon).healing){
                damageType = " (HEALING)";
            }
            elem = (recipe.itemToCraft as FishingWeapon).element.ToString();
        }
        else if (recipe.itemToCraft is FishingArmor){
            itemType = "Armor";
            atk = (recipe.itemToCraft as FishingArmor).ATK;
            def = (recipe.itemToCraft as FishingArmor).DEF;
            if((recipe.itemToCraft as FishingArmor).healing){
                damageType = " (HEALING)";
            }
            elem = (recipe.itemToCraft as FishingArmor).element.ToString();
        }
        else if (recipe.itemToCraft is FishingMaterial){
            itemType = "Material";
        }
        else if (recipe.itemToCraft is FishingLure){
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
        if(recipe == null){Debug.Log("Null item, wtf????");return;}
        
        if(inv.CheckIfCraftable(recipe)){
            craftableText.text = "V";
        }
        else{
            craftableText.text = "X";
        }
        
        icon.sprite = recipe.itemToCraft.icon;
    
    }

    public void InventoryClick(){
        if(recipe.itemToCraft is FishingArmor){
            if(inv.armor1 == inv.defaultArmor){
                inv.armor1 = recipe.itemToCraft as FishingArmor;
                inv.Remove(recipe.itemToCraft);
            }
            else{
                inv.Add(inv.armor1);
                inv.armor1 = recipe.itemToCraft as FishingArmor;
                inv.Remove(recipe.itemToCraft);
            }
            uii.CreateInventoryDisplay();
        }
        else if(recipe.itemToCraft is FishingWeapon){
            uii.clicked = recipe.itemToCraft as FishingWeapon;
            uii.WeaponPopup.gameObject.SetActive(true);
        }
    }

}
