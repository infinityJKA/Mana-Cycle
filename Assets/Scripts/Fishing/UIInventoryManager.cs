using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UIInventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();
    public FishingInventory inventory;
    public Image bigImage;
    public TextMeshProUGUI bigName;
    public TextMeshProUGUI Description;
    public string mode = "ALL";

    public void setMode(string s){
        mode = s;
    }

    void OnEnable(){
        CreateInventoryDisplay();
    }

    public void CreateInventoryDisplay(){
        // Destroy outdated inventory display
        foreach(Transform c in transform){
            Destroy(c.gameObject);
        }
        List<ItemInInventory> itemsToShow = GenerateInventoryList();
        itemSlots = new List<ItemSlot>(itemsToShow.Count);

        // Create new inventory display
        for(int i = 0; i < itemSlots.Capacity; i++){
            CreateItemSlot();
        }

        for(int i = 0; i < itemSlots.Count; i++){
            itemSlots[i].item = itemsToShow[i];
            itemSlots[i].DrawSlot();
        }

    }

    private List<ItemInInventory> GenerateInventoryList(){         // This is for filtering
        List<ItemInInventory> inv = inventory.inventory;
        List<ItemInInventory> newInv = new List<ItemInInventory>();

        // idk how to do this more efficiently since it is dealing with class types
        if(mode == "TOMES"){
            for(int i = 0; i < inventory.inventory.Count; i++){
                if(inv[i].itemData is FishingTome){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "WEAPONS"){
            for(int i = 0; i < inventory.inventory.Count; i++){
                if(inv[i].itemData is FishingWeapon){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "ARMOR"){
            for(int i = 0; i < inventory.inventory.Count; i++){
                if(inv[i].itemData is FishingArmor){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "MATERIALS"){
            for(int i = 0; i < inventory.inventory.Count; i++){
                if(inv[i].itemData is FishingMaterial){
                    newInv.Add(inv[i]);
                }
            }
        }
        else if(mode == "LURES"){
            for(int i = 0; i < inventory.inventory.Count; i++){
                if(inv[i].itemData is FishingLure){
                    newInv.Add(inv[i]);
                }
            }
        }
        else{
            newInv = inv;
        }

        return newInv;
    }

    private void CreateItemSlot(){
        GameObject newSlot = Instantiate(slotPrefab);
        newSlot.transform.SetParent(transform, false);

        ItemSlot newSlotComponent = newSlot.GetComponent<ItemSlot>();

        itemSlots.Add(newSlotComponent);
    }


}
