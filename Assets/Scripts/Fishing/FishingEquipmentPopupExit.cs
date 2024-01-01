using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;
using System;
using Animation;

public class FishingEquipmentPopupExit : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public GameObject highlight;
    public GameObject PopupScreen;

    public void OnPointerEnter(PointerEventData pointerEventData){
        highlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData){
        highlight.SetActive(false);
    }

    public void Click(){
        highlight.SetActive(false);
        PopupScreen.SetActive(false);
    }
    


}
