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
    public CraftingManager cm;
    public GameObject cursor;
    public FishingInventory inv;
    public RequiredCraftingItemsDisplay rq;
    public TextMeshProUGUI bigCraftingText;
    [SerializeField] TMP_ColorGradient redGradient;
    [SerializeField] TMP_ColorGradient greenGradient;


    public void OnEnable(){
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        cm = GameObject.Find("Crafting Manager").GetComponent<CraftingManager>();
        bigCraftingText = cm.craftingText;
        rq = GameObject.Find("Required Items").GetComponent<RequiredCraftingItemsDisplay>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        cm.bigImage.sprite = icon.sprite;
        cm.bigName.text = recipe.itemToCraft.itemName;
        
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

        cm.Description.text =
        "Value: "+ val.ToString()+
        "\nSTR: "+ atk.ToString()+ damageType+
        "\nDEF: "+ def.ToString()+
        "\nElement: "+ elem+
        "\nItem Type: "+ itemType
        +"\n\n"+desc;
        
        cursor.SetActive(true);
        // Debug.Log(this.gameObject.name + " was selected");

        if(inv.CheckIfCraftable(recipe)){
            bigCraftingText.text = "CRAFTABLE";
            bigCraftingText.colorGradientPreset  = greenGradient;
        }
        else{
            bigCraftingText.text = "NOT ENOUGH MATERIALS";
            bigCraftingText.colorGradientPreset  = redGradient;
        }

        cm.clicked = recipe;
        rq.UpdateDisplay();
    }

    public void OnPointerExit(PointerEventData pointerEventData){
        cursor.SetActive(false);
    }

    public void DrawSlot(){
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        
        if(recipe == null){Debug.Log("Null recipe, wtf????");return;}
        
        if(inv.CheckIfCraftable(recipe)){
            craftableText.text = "YES";
            craftableText.colorGradientPreset  = greenGradient;
        }
        else{
            craftableText.text = "NO";
            craftableText.colorGradientPreset  = redGradient;
        }
        
        icon.sprite = recipe.itemToCraft.icon;
    
    }

    public void Craft(){
        if(inv.CheckIfCraftable(recipe)){
            // remove items needed to craft (why am i just now starting to comment after like a hundred functions)
            for(int i = 0; i < recipe.requiredItems.Count; i++){
                for(int t = 0; t < recipe.requiredItemCount[i]; t++){
                    cm.inventory.Remove(recipe.requiredItems[i]);
                }
            }
            // add the crafted item
            cm.inventory.Add(recipe.itemToCraft);
            // popup menu
            cm.craftedPopup.SetActive(true);
            cm.craftedPopupText.text = "You crafted " + recipe.itemToCraft.name +"!";
            // refresh inventory
            cm.CreateCraftingDisplay();
        }

    }

}
