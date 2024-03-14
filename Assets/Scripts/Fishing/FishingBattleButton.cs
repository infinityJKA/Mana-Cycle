using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishingBattleButton : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public Image weaponImage,used,cursor;
    public int itemNumber;

    public void OnPointerEnter(PointerEventData pointerEventData){
        cursor.gameObject.SetActive(true);
    }
    public void OnPointerExit(PointerEventData pointerEventData){
        cursor.gameObject.SetActive(false);
    }



}


