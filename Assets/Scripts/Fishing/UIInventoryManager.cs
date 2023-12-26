using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIInventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();
    public FishingInventory inventory;
    public Image bigImage;
    public TextMeshProUGUI bigName;
    public TextMeshProUGUI Description;


    void OnEnable(){
        CreateInventoryDisplay();
    }

    public void CreateInventoryDisplay(){
        // Destroy outdated inventory display
        foreach(Transform c in transform){
            Destroy(c.gameObject);
        }
        itemSlots = new List<ItemSlot>(inventory.inventory.Count);

        // Create new inventory display
        for(int i = 0; i < itemSlots.Capacity; i++){
            CreateItemSlot();
        }

        for(int i = 0; i < itemSlots.Count; i++){
            itemSlots[i].item = inventory.inventory[i];
            itemSlots[i].DrawSlot();
        }

    }

    private void CreateItemSlot(){
        GameObject newSlot = Instantiate(slotPrefab);
        newSlot.transform.SetParent(transform, false);

        ItemSlot newSlotComponent = newSlot.GetComponent<ItemSlot>();

        itemSlots.Add(newSlotComponent);
    }


}
