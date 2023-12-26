using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ItemInInventory
{
    public FishingItem itemData;
    public int stackSize;

    public ItemInInventory(FishingItem item){
        itemData = item;
        IncreaseStack();
    }

    public void IncreaseStack(){
        stackSize++;
    }

    public void DecreaseStack(){
        stackSize--;
    }

}
