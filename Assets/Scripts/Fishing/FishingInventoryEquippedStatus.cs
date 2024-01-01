using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class FishingInventoryEquippedStatus : MonoBehaviour
{
    public FishingEquippedInteract Left,Body,Right;
    public TextMeshProUGUI StatsText;

    void OnEnable(){
        UpdateDisplay();
    }

    public void UpdateDisplay(){
        UpdateSprites();
        UpdateText();
    }

    private void UpdateSprites(){
        Left.Generate();
        Body.Generate();
        Right.Generate();
    }
    private void UpdateText(){
        string l = "ERROR";string b = "ERROR";string r = "ERROR";
        if((Left.equippedItem as FishingWeapon).healing){l="HEAL";}else{l="ATK";}
        if((Body.equippedItem as FishingArmor).healing){l="HEAL";}else{l="ATK";}
        if((Right.equippedItem as FishingWeapon).healing){l="HEAL";}else{l="ATK";}

        StatsText.text =
        "Left Hand: "+((Left.equippedItem as FishingWeapon).ATK).ToString()+" STR | "+((Left.equippedItem as FishingWeapon).DEF).ToString()+" DEF | "+l+"\n"+
        "Body: "+((Body.equippedItem as FishingArmor).ATK).ToString()+" STR | "+((Body.equippedItem as FishingArmor).DEF).ToString()+" DEF | "+b+"\n"+
        "Right Hand: "+((Right.equippedItem as FishingWeapon).ATK).ToString()+" STR | "+((Right.equippedItem as FishingWeapon).DEF).ToString()+" DEF | "+r;
    }
}
