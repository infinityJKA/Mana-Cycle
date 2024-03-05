using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class CraftingManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<CraftingSlot> craftingSlots = new List<CraftingSlot>();
    public FishingInventory inventory;
    public Image bigImage;
    public TextMeshProUGUI bigName;
    public TextMeshProUGUI Description;
    public string mode = "ALL";
    public CraftingRecipe clicked;
    public FishingInventoryBaitAndGoldCounter FIBAGC;
    public TextMeshProUGUI craftingText;

    public void setMode(string s){
        mode = s;
    }

    void OnEnable(){
        inventory = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        CreateCraftingDisplay();
    }

    public void CreateCraftingDisplay(){
        // Destroy outdated inventory display
        foreach(Transform c in transform){
            Destroy(c.gameObject);
        }
        List<CraftingRecipe> itemsToShow = GenerateCraftingList();
        craftingSlots = new List<CraftingSlot>(itemsToShow.Count);

        // Create new inventory display
        for(int i = 0; i < craftingSlots.Capacity; i++){
            CreateCraftingSlot();
        }

        for(int i = 0; i < craftingSlots.Count; i++){
            craftingSlots[i].recipe = itemsToShow[i];
            craftingSlots[i].DrawSlot();
        }

        // Update the equipped items display
        //FIES.UpdateDisplay();  ADD CRAFTING ITEM VERSION OF THIS HERE =====================================================

        // Update the bait/gold counter
        FIBAGC.UpdateText();

    }

    private List<CraftingRecipe> GenerateCraftingList(){         // This is for filtering
        List<CraftingRecipe> inv = inventory.craftingList;
        List<CraftingRecipe> newInv = new List<CraftingRecipe>();

        // idk how to do this more efficiently since it is dealing with class types
        if(mode == "TOMES"){
            for(int i = 0; i < inventory.craftingList.Count; i++){
                if(inv[i].itemToCraft is FishingTome){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "WEAPONS"){
            for(int i = 0; i < inventory.craftingList.Count; i++){
                if(inv[i].itemToCraft is FishingWeapon){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "ARMOR"){
            for(int i = 0; i < inventory.craftingList.Count; i++){
                if(inv[i].itemToCraft is FishingArmor){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "MATERIALS"){
            for(int i = 0; i < inventory.craftingList.Count; i++){
                if(inv[i].itemToCraft is FishingMaterial){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "LURES"){
            for(int i = 0; i < inventory.craftingList.Count; i++){
                if(inv[i].itemToCraft is FishingLure){
                    newInv.Add(inv[i]);
                }
            }
        }
        else{
            newInv = inv;
        }

        return newInv;
    }

    private void CreateCraftingSlot(){
        GameObject newSlot = Instantiate(slotPrefab);
        newSlot.transform.SetParent(transform, false);

        CraftingSlot newSlotComponent = newSlot.GetComponent<CraftingSlot>();

        craftingSlots.Add(newSlotComponent);
    }


}
