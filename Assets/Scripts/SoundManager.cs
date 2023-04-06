using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{   
    public static SoundManager Instance;
    [SerializeField] private AudioSource musicSource, effectSource;
    
    // mainMenuMusic used when returning back to the menu from a match
    public AudioClip mainMenuMusic;

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

    /** Set the background music. If the passed song is already playing, do not replay */
    public void SetBGM(AudioClip clip) {
        if (musicSource.clip == clip) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    /** Load the song but do not play it yet. */
    public void LoadBGM(AudioClip clip) {
        musicSource.clip = clip;
    }

    public void UnpauseBGM() {
        musicSource.Play();
    }

    public void PauseBGM() {
        musicSource.Pause();
    }

    public void StopBGM() {
        musicSource.Stop();
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
        PlayerPrefs.SetFloat("musVolumeKey", musicSource.volume);
        PlayerPrefs.SetFloat("sfxVolumeKey", effectSource.volume);
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
