using System.Collections;
using System.Collections.Generic;
using SoloMode;
using UnityEngine;
using UnityEngine.UI;

// used on sliders / options to set player prefs through unity event trigger system
namespace MainMenu{
    public class PlayerPrefSetter : MonoBehaviour
    {
        [SerializeField] private string key = "defaultKey";

        // optional slider to sync to settings
        [SerializeField] private Slider slider;

        public void SetPrefFloat(float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public void Sync()
        {
            if (slider != null) slider.value = PlayerPrefs.GetFloat(key);
        }
    }

}