using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


 public class MenuButtonCam : MonoBehaviour, ISelectHandler
 {
    public CinemachineVirtualCamera thisCam;
    public CinemachineBrain brain;
    public TMP_Text textBox;
    public string text;

    public void OnSelect(BaseEventData eventData){
        // if(eventData.selectedObject == this.gameObject){
            brain.ActiveVirtualCamera.Priority = 1;
            thisCam.Priority = 30;
            textBox.text = text;
        // } 
    }
}
