using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using Sound;
using VersusMode;
using UnityEngine.InputSystem.UI;

namespace MainMenu {
    /// <summary>
    /// Controls the cinemachine camera in the 3d menu.
    /// <summary>
    public class SettingsSelectable : MonoBehaviour, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData){
            // if a selector somehow selects this without being in settings mode,
            // return them to their last selected icon
            var selector = eventData.currentInputModule.gameObject.GetComponent<CharSelector>();
            if (!selector.settingsDisplayed)
            {
                StartCoroutine(DeselectAfterFrame(eventData, selector));
            } else
            {
                selector.PlaySettingsMoveSFX();
            }
        }

        IEnumerator DeselectAfterFrame(BaseEventData eventData, CharSelector selector)
        {
            yield return new WaitForEndOfFrame();
            eventData.currentInputModule.gameObject.GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(
                    selector.selectedIcon.gameObject);
        }
    }
}