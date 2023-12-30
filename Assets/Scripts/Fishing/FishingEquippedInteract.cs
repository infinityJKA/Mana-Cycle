using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;
using System;

public class FishingEquippedInteract : MonoBehaviour
{
    public GameObject highlight;
    public Image icon;
    public FishingInventory inv;
    
    public FishingItem equippedItem;
    public string itemLocation;

    void Start(){
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        Generate();
    }

    void OnEnable(){
        Generate();
    }

    public void Generate(){
        // Pulling the data
        if(itemLocation == "left"){
		    try {equippedItem = inv.weapon1;}
            catch (NullReferenceException e) {equippedItem = null;}  
        }
        else if(itemLocation == "right"){
            try {equippedItem = inv.weapon2;}
            catch (NullReferenceException e) {equippedItem = null;}
        }
        else{
            try {equippedItem = inv.armor1;}
            catch (NullReferenceException e) {equippedItem = null;}
        }

        // Drawing the icon
        if(equippedItem != null){
            icon.gameObject.SetActive(true);
            icon.sprite = equippedItem.icon;
        }
        else{
            icon.gameObject.SetActive(false);
        }
    }

    public void Dequip(){
        if(equippedItem != null){
            inv.Add(equippedItem);
            equippedItem = null;
            Generate();
        }
    }


}
