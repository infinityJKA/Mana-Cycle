using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{   
    public static SoundManager Instance;
    public AudioSource musicSource, effectSource;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }

        Load();
    }

    public void PlaySound(AudioClip clip, float pitch = 1f)
    {
        // create a new gameobject with an audiosource, to avoid interfering with other sound effects
        GameObject tempEffectSource = new GameObject("tempEffectSource");
        tempEffectSource.AddComponent<AudioSource>();
        tempEffectSource.GetComponent<AudioSource>().pitch = pitch;
        tempEffectSource.GetComponent<AudioSource>().PlayOneShot(clip, effectSource.volume);
        Destroy(tempEffectSource, clip.length);
    }

    // sliders cannot call functions with more than 1 param (?)
    public void PlaySound(AudioClip clip)
    {
        PlaySound(clip, 1f);
    }

    public void ChangeMusicVolume(float value)
    {
        musicSource.volume = value;
        Save();
    }

    public void ChangeSFXVolume(float value)
    {

        effectSource.volume = value;
        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("musVolumeKey", effectSource.volume);
        PlayerPrefs.SetFloat("musVolumeKey", effectSource.volume);
    }

    public void Load()
    {
        // music
        if (!PlayerPrefs.HasKey("musVolumeKey"))
        {
            PlayerPrefs.SetFloat("musVolumeKey", 0.5f);
        }
        else
        {
            musicSource.volume = PlayerPrefs.GetFloat("musVolumeKey");
        }

        // sfx
        if (!PlayerPrefs.HasKey("sfxVolumeKey"))
        {
            PlayerPrefs.SetFloat("sfxVolumeKey", 0.5f);
        }
        else
        {
            effectSource.volume = PlayerPrefs.GetFloat("sfxVolumeKey");
        }
        
    }

}
