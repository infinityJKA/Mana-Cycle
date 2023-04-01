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
        // create a new gameobject with an audiosource, to avoid interfering with other sound effects
        GameObject tempEffectSource = new GameObject("tempEffectSource");
        tempEffectSource.AddComponent<AudioSource>();
        tempEffectSource.GetComponent<AudioSource>().pitch = pitch;
        tempEffectSource.GetComponent<AudioSource>().PlayOneShot(clip);
        Destroy(tempEffectSource, clip.length);
    }

    public void ChangeMusicVolume(float value){
        musicSource.volume = value;
    }

    public void ChangeSFXVolume(float value){

        effectSource.volume = value;
    }

}
