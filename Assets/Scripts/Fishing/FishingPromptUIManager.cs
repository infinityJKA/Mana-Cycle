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
    public TextMeshProUGUI baitBeforeAndAfter;
    public int baitToUse;
    void OnEnable(){
        UnequipLure();
        resetBait();
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
            CreateLureSlot();
        }

        for(int i = 0; i < itemSlots.Count; i++){
            itemSlots[i].item = itemsToShow[i];
            itemSlots[i].DrawSlot();
        }

        // Update the bait/gold counter
        FIBAGC.UpdateText();

        // Bait before and after text
        generateBaitText();

    }

    private List<ItemInInventory> GenerateInventoryList(){         // This is for filtering
        List<ItemInInventory> inv = inventory.inventory;
        List<ItemInInventory> newInv = new List<ItemInInventory>();

        for(int i = 0; i < inventory.inventory.Count; i++){
            if(inv[i].itemData is FishingLure){
                Debug.Log(inv[i].itemData.name + " IS FISHING LURE");
                newInv.Add(inv[i]);
            }
            else{
                Debug.Log(inv[i].itemData.name + " is not fishing lure");
            }
        }

        return newInv;
    }

    private void CreateLureSlot(){
        GameObject newSlot = Instantiate(slotPrefab);
        newSlot.transform.SetParent(transform, false);

        LureSlot newSlotComponent = newSlot.GetComponent<LureSlot>();

        itemSlots.Add(newSlotComponent);
    }

    public void UnequipLure(){
        lureToUse = null;
        bigImage.sprite = inventory.defaultArmor.icon;  
        bigName.text = "[NO LURE]";
        Description.text = "[No lure equipped]\n\nClick a lure to equip it. Lures are used up upon fishing.";
        CreateInventoryDisplay();
    }

    public void increaseBaitToUse(int b){
        if(baitToUse + b > inventory.bait){
            baitToUse = inventory.bait;
        }
        else{
            baitToUse += b;
        }
        generateBaitText();
    }

    public void decreaseBaitToUse(int b){
        if(inventory.bait >= 1){
            if(baitToUse - b < 1){
                baitToUse = 1;
            }
            else{
                baitToUse -= b;
            }
        }
        generateBaitText();
    }

    public void resetBait(){
        if(inventory.bait >= 1){
            baitToUse = 1;
        }
        else{
            baitToUse = 0;
        }
        generateBaitText();
    }

    private void generateBaitText(){
        if(inventory.bait >= 1){
            baitBeforeAndAfter.text = "use "+baitToUse+" bait";
        }
        else{
            baitBeforeAndAfter.text = "not enough bait";
        }
    }


}
