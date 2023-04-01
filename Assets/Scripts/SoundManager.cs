using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{   public static SoundManager Instance;
    
    public AudioSource musicSource, effectSource;

    void Awake(){
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip, float pitch = 1f){
        effectSource.pitch = pitch;
        effectSource.PlayOneShot(clip);
    }

    public void ChangeMusicVolume(float value){
        musicSource.volume = value;
    }

    public void ChangeSFXVolume(float value){

        effectSource.volume = value;
    }

}
