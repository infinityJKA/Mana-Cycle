using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI stackSizeText;
    public ItemInInventory item;

    public void DrawSlot(){
        if(item == null){Debug.Log("Null item, wtf????");return;}

        stackSizeText.text = item.stackSize.ToString();
        icon.sprite = item.itemData.icon;
    
    }

}
