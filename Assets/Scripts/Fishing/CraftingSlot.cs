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
    private Color g1,g2,g3,g4,r1,r2,r3,r4;

    public void OnEnable(){
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        cm = GameObject.Find("Crafting Manager").GetComponent<CraftingManager>();
        bigCraftingText = cm.craftingText;
        rq = GameObject.Find("Required Items").GetComponent<RequiredCraftingItemsDisplay>();

        g1 = new Color(0f, 255f, 155f);
        g3 = new Color(248f, 255f, 0f);
        g2 = new Color(0f, 255f, 155f);
        g4 = new Color(248f, 255f, 0f);

        r1 = new Color(142f, 0f, 0f);
        r2 = new Color(255, 35f, 240f);
        r3 = new Color(142f, 0f, 0f);
        r4 = new Color(255, 35f, 240f);

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
            bigCraftingText.colorGradient = new VertexGradient(g1,g2,g3,g4);
        }
        else{
            bigCraftingText.text = "NOT ENOUGH MATERIALS";
            bigCraftingText.colorGradient = new VertexGradient(r1,r2,r3,r4);
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
        }
        else{
            craftableText.text = "NO";
        }
        
        icon.sprite = recipe.itemToCraft.icon;
    
    }

    public void InventoryClick(){
        // if(recipe.itemToCraft is FishingArmor){
        //     if(inv.armor1 == inv.defaultArmor){
        //         inv.armor1 = recipe.itemToCraft as FishingArmor;
        //         inv.Remove(recipe.itemToCraft);
        //     }
        //     else{
        //         inv.Add(inv.armor1);
        //         inv.armor1 = recipe.itemToCraft as FishingArmor;
        //         inv.Remove(recipe.itemToCraft);
        //     }
        //     cm.CreateCraftingDisplay();
        // }
        // else if(recipe.itemToCraft is FishingWeapon){
        //     cm.clicked = recipe.itemToCraft as FishingWeapon;
        //     cm.WeaponPopup.gameObject.SetActive(true);
        // }
    }

}
