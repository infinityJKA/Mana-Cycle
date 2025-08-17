using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// used on sliders / options to set player prefs through unity event trigger system
public class PlayerPrefSetter : MonoBehaviour
{
    [SerializeField] private string key = "defaultKey";
    [SerializeField] private float defaultValue = 0f;

    // optional selectables to sync to settings
    [SerializeField] private Slider slider;
    [SerializeField] private Toggle toggle;

    public void SetPrefFloat(float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public void SetPrefInt(int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public void SetPrefBool(bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public void Sync()
    {
        if (slider != null) slider.value = PlayerPrefs.GetFloat(key, defaultValue);
        if (toggle != null) toggle.isOn = PlayerPrefs.GetInt(key, (int) defaultValue) == 1;
    }

}