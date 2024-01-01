using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;
using System;
using Animation;

public class FishingEquipPopupBubble : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public GameObject highlight;
    public Image icon;
    public FishingInventory inv;
    
    public FishingWeapon bubbleWeapon;
    public UIInventoryManager uii;
    public GameObject PopupScreen;
    public string hand;

    void OnEnable(){
        uii = GameObject.Find("UI Inventory Manager").GetComponent<UIInventoryManager>();
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData){
        highlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData){
        highlight.SetActive(false);
    }

    public void Click(){
        if(hand == "LEFT"){
            if(inv.weapon1 != inv.defaultWeapon){
                inv.Add(inv.weapon1);
            }
            inv.weapon1 = uii.clicked;
        }
        else{
            if(inv.weapon2 != inv.defaultWeapon){
                inv.Add(inv.weapon2);
            }
            inv.weapon2 = uii.clicked;
        }
        inv.Remove(uii.clicked);
        uii.CreateInventoryDisplay();
        highlight.SetActive(false);
        PopupScreen.SetActive(false);
    }
    


}
