using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingInventory : MonoBehaviour
{
    public List<ItemInInventory> inventory = new List<ItemInInventory>();
    private Dictionary<FishingItem,ItemInInventory> itemDictionary = new Dictionary<FishingItem,ItemInInventory>();

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

}
