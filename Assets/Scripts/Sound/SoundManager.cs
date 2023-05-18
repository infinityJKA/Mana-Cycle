using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sound {
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

        public void PlaySound(AudioClip clip, float pitch = 1f, float pan = 0f, float volumeScale = 1f, bool important = true)
        {
            // don't play if sound limit exceeded
            // if (effectSource.transform.childCount >= 19) return;

            // if this is an unimportant sound, don't play if there are a lot of sfx playing
            // (things like move, rotate, place are not important)
            // if (effectSource.transform.childCount >= 14 && !important) return; 

            // If sound limit exceeded, remove the topmost sound effect
            if (effectSource.transform.childCount >= 19) {
                Destroy(effectSource.transform.GetChild(0).gameObject);
            }

            // create a new gameobject with an audiosource, to avoid interfering with other sound effects
            var tempEffectSource = new GameObject("tempEffectSource").AddComponent<AudioSource>();
            tempEffectSource.transform.SetParent(effectSource.transform);
            tempEffectSource.pitch = pitch;
            tempEffectSource.panStereo = pan;
            tempEffectSource.volume = Mathf.Clamp(tempEffectSource.volume * volumeScale, 0, 1);
            tempEffectSource.PlayOneShot(clip, Instance.effectSource.volume);
            Destroy(tempEffectSource.gameObject, clip.length);
        }

        /** Set the background music. If the passed song is already playing, do not replay */
        public void SetBGM(AudioClip clip) {
            if (Instance.musicSource.clip == clip) return;

            Instance.musicSource.Stop();
            Instance.musicSource.clip = clip;
            Instance.musicSource.Play();
        }

        /** Load the song but do not play it yet. */
        public void LoadBGM(AudioClip clip) {
            Instance.musicSource.Stop();
            Instance.musicSource.clip = clip;
        }

        public void PlayBGM() {
            Instance.musicSource.Play();
        }

        public void UnpauseBGM() {
            Instance.musicSource.UnPause();
        }

        public void PauseBGM() {
            Instance.musicSource.Pause();
        }

        public void StopBGM() {
            Instance.musicSource.Stop();
        }

        public void UnloadBGM() {
            StopBGM();
            Instance.musicSource.clip = null;
        }

        // sliders cannot call functions with more than 1 param (?)
        public void PlaySound(AudioClip clip)
        {
            PlaySound(clip, 1f);
        }

        public void ChangeMusicVolume(float value)
        {
            Instance.musicSource.volume = value;
            Save();
        }

        public void ChangeSFXVolume(float value)
        {

            Instance.effectSource.volume = value;
            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetFloat("musVolumeKey", Instance.musicSource.volume);
            PlayerPrefs.SetFloat("sfxVolumeKey", Instance.effectSource.volume);
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
                Instance.musicSource.volume = PlayerPrefs.GetFloat("musVolumeKey");
            }

            // sfx
            if (!PlayerPrefs.HasKey("sfxVolumeKey"))
            {
                PlayerPrefs.SetFloat("sfxVolumeKey", 0.5f);
            }
            else
            {
                Instance.effectSource.volume = PlayerPrefs.GetFloat("sfxVolumeKey");
            }
            
        }

    }
}