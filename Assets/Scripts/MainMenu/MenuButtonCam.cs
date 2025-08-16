using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using Sound;

namespace MainMenu {
    /// <summary>
    /// Controls the cinemachine camera in the 3d menu.
    /// <summary>
    public class MenuButtonCam : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public CinemachineVirtualCamera thisCam;
        public CinemachineBrain brain;
        // public string text;

        // Components to only be enabled when this item is selected
        public MonoBehaviour[] enableWhenSelected;

        // public GameObject selectSFX, clickSFX;
        public int buttonIndex;

        public void OnSelect(BaseEventData eventData){
            Storage.lastMainMenuItem = buttonIndex;

            // if(eventData.selectedObject == this.gameObject){
                // Instantiate(selectSFX);
                if (brain && brain.ActiveVirtualCamera != null) brain.ActiveVirtualCamera.Priority = 1;
                if (thisCam) thisCam.Priority = 30;
                // textBox.text = text;
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

        // public void PlayClickSFX(){
        //     Instantiate(clickSFX);
        // }
    }
}