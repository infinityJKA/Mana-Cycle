using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class FishingPromptUIManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<LureSlot> itemSlots = new List<LureSlot>();
    public FishingInventory inventory;
    public Image bigImage;
    public TextMeshProUGUI bigName;
    public TextMeshProUGUI Description;
    public FishingItem lureToUse;
    public FishingInventoryBaitAndGoldCounter FIBAGC;
    void OnEnable(){
        CreateInventoryDisplay();
    }

    public void CreateInventoryDisplay(){
        // Destroy outdated inventory display
        foreach(Transform c in transform){
            Destroy(c.gameObject);
        }
        List<ItemInInventory> itemsToShow = GenerateInventoryList();
        itemSlots = new List<LureSlot>(itemsToShow.Count);

        // Create new inventory display
        for(int i = 0; i < itemSlots.Capacity; i++){
            CreateItemSlot();
        }

        for(int i = 0; i < itemSlots.Count; i++){
            itemSlots[i].item = itemsToShow[i];
            itemSlots[i].DrawSlot();
        }

        // Update the bait/gold counter
        FIBAGC.UpdateText();

    }

    private List<ItemInInventory> GenerateInventoryList(){         // This is for filtering
        List<ItemInInventory> inv = inventory.inventory;
        List<ItemInInventory> newInv = new List<ItemInInventory>();

        for(int i = 0; i < inventory.inventory.Count; i++){
            if(inv[i].itemData is FishingLure){
                newInv.Add(inv[i]);
            }
        }

        return newInv;
    }

    private void CreateItemSlot(){
        GameObject newSlot = Instantiate(slotPrefab);
        newSlot.transform.SetParent(transform, false);

        LureSlot newSlotComponent = newSlot.GetComponent<LureSlot>();

        itemSlots.Add(newSlotComponent);
    }


}
