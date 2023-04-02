using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


 public class MenuButtonCam : MonoBehaviour, ISelectHandler, IDeselectHandler
 {
    public CinemachineVirtualCamera thisCam;
    public CinemachineBrain brain;
    public TMP_Text textBox;
    public string text;

    // Components to only be enabled when this item is selected
    public MonoBehaviour[] enableWhenSelected;

    public AudioClip selectSFX;
    public AudioClip clickSFX;

    public void OnSelect(BaseEventData eventData){
        // if(eventData.selectedObject == this.gameObject){
            SoundManager.Instance.PlaySound(selectSFX);
            if (brain.ActiveVirtualCamera != null) brain.ActiveVirtualCamera.Priority = 1;
            thisCam.Priority = 30;
            textBox.text = text;
        // }

        foreach (MonoBehaviour comp in enableWhenSelected) {
            comp.enabled = true;
        }
    }

    public void OnDeselect(BaseEventData eventData) {
        foreach (MonoBehaviour comp in enableWhenSelected) {
            comp.enabled = false;
        }
    }

    public void PlayClickSFX(){
        SoundManager.Instance.PlaySound(clickSFX);
    }
}
