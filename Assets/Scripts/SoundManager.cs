using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{   public static SoundManager Instance;
    
    public AudioSource musicSource, effectSource;
    [SerializeField] private AudioClip sliderSFX;

    void Awake(){
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip){
        effectSource.PlayOneShot(clip);
    }

    public void ChangeMusicVolume(float value){
        musicSource.volume = value;
    }

    public void ChangeSFXVolume(float value){

        effectSource.volume = value;
    }

}
