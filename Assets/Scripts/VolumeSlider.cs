using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private SoundManager soundManager;
    [SerializeField] private Slider slider;
    [SerializeField] private string valueKey;

    void Start()
    {
        // soundmanager is dontdestroyonload, so we need to re-find it on scene reload. otherwise it will detach from slider serialization
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        Debug.Log(soundManager);

        // set slider positions
        if (PlayerPrefs.HasKey(valueKey))
        {
            slider.value = PlayerPrefs.GetFloat(valueKey);
        }
    }

    public void setMusVolume(float value)
    {
        soundManager.ChangeMusicVolume(value);
    }

    public void setSFXVolume(float value)
    {
        soundManager.ChangeSFXVolume(value);
    }

    public void PlaySound(AudioClip clip)
    {
        soundManager.PlaySound(clip, 1f);
    }
}
