using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FishingInventory : MonoBehaviour
{
    public List<ItemInInventory> inventory = new List<ItemInInventory>();
    public int bait;
    public int gold;
    public Dictionary<FishingItem,ItemInInventory> itemDictionary = new Dictionary<FishingItem,ItemInInventory>();

    public FishingWeapon weapon1;
    public FishingWeapon weapon2;
    public FishingArmor armor1;

    public FishingWeapon defaultWeapon;
    public FishingArmor defaultArmor;
    public List<CraftingRecipe> craftingList = new List<CraftingRecipe>();

    // public List<FishingItem> GetInventoryList(){
    //     List<FishingItem> L = new List<FishingItem>();
    //     foreach(var i in itemDictionary){
    //         L.Add(i.key)
    //     }
    // }

    public void Add(FishingItem itemData){

        // If the item already has a stack in your inventory
        if(itemDictionary.TryGetValue(itemData, out ItemInInventory item)){
            item.IncreaseStack();
        }

        // If the item doesn't have a stack yet
        else{
            ItemInInventory newItem = new ItemInInventory(itemData);
            inventory.Add(newItem);
            itemDictionary.Add(itemData,newItem);
        }
    }

    public void Remove(FishingItem itemData){

        // If the item already has a stack in your inventory
        if(itemDictionary.TryGetValue(itemData, out ItemInInventory item)){
            item.DecreaseStack();
            
            // Check if the stack should still exist
            if(item.stackSize <= 0){
                inventory.Remove(item);
                itemDictionary.Remove(itemData);
            }

        }

        else{
            
        }
    }

    public bool CheckIfCraftable(CraftingRecipe r){
        bool craftable = true;
        foreach(FishingItem required in r.requiredItems){
            bool itemRequirementMet = false;
            foreach(ItemInInventory i in inventory){
                if(i.itemData == required){
                    if(i.stackSize >= r.requiredItemCount[r.requiredItems.IndexOf(required)]){
                        itemRequirementMet = true;
                    }
                }
            }
            if(!itemRequirementMet){
                craftable = false;
            }
        }

        return craftable;
    }

}
