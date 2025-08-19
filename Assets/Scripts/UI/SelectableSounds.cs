using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Menus
{
    public class SelectableSounds : MonoBehaviour, ISelectHandler, ISubmitHandler
    {
        [SerializeField] private GameObject selectSFX;
        [SerializeField] private GameObject submitSFX;

        public void OnSelect(BaseEventData eventData)
        {
            if (selectSFX) Instantiate(selectSFX);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (submitSFX) Instantiate(submitSFX);
        }
    }
}