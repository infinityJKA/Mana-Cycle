using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingWeaponEquipPopup : MonoBehaviour
{
    public UIInventoryManager uii;
    public FishingInventory inv;
    public FishingEquipPopupBubble left;
    public FishingEquipPopupBubble right;

    void OnEnable(){
        uii = GameObject.Find("UI Inventory Manager").GetComponent<UIInventoryManager>();
        inv = GameObject.Find("Inventory").GetComponent<FishingInventory>();
        left.hand = "LEFT";
        right.hand = "RIGHT";
        left.bubbleWeapon = inv.weapon1;
        left.icon.sprite = inv.weapon1.icon;
        right.bubbleWeapon = inv.weapon2;
        right.icon.sprite = inv.weapon2.icon;
    }

}
