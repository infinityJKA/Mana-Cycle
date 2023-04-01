using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSlider : MonoBehaviour
{
    private SoundManager soundManager;

    void Start()
    {
        // soundmanager is dontdestroyonload, so we need to re-find it on scene reload. otherwise it will detach from slider serialization
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        Debug.Log(soundManager);
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
