using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class RequiredCraftingItemsDisplay : MonoBehaviour
{
    public List<RequiredItemDisplay> requiredItemDisplays;
    public TextMeshProUGUI StatsText;
    public CraftingManager cm;
    [SerializeField] TMP_ColorGradient redGradient;
    [SerializeField] TMP_ColorGradient greenGradient;

    void OnEnable(){
        UpdateDisplay();
    }

    public void UpdateDisplay(){
        UpdateSprites();
    }

    private void UpdateSprites(){
        for(int i = 0; i < requiredItemDisplays.Count; i++){
            if(i < cm.clicked.requiredItems.Count){
                UpdateSprite(i,requiredItemDisplays[i]);
            }
            else{
                requiredItemDisplays[i].countText.text = "";
                requiredItemDisplays[i].icon.sprite = null;
            }
        }
    }

    private void UpdateSprite(int i, RequiredItemDisplay r){
        r.countText.text = cm.inventory.CheckCount(cm.clicked.requiredItems[i])+"/"+cm.clicked.requiredItemCount[i];
        r.icon.sprite = cm.clicked.requiredItems[i].icon;
        if(cm.inventory.CheckCount(cm.clicked.requiredItems[i]) >= cm.clicked.requiredItemCount[i]){
            r.countText.colorGradientPreset = greenGradient;
        }
        else{
            r.countText.colorGradientPreset = redGradient;
        }
    }

}
