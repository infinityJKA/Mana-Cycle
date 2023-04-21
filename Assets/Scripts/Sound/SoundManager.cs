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

        public void PlaySound(AudioClip clip, float pitch = 1f)
        {
            // create a new gameobject with an audiosource, to avoid interfering with other sound effects
            GameObject tempEffectSource = new GameObject("tempEffectSource");
            tempEffectSource.AddComponent<AudioSource>();
            tempEffectSource.GetComponent<AudioSource>().pitch = pitch;
            tempEffectSource.GetComponent<AudioSource>().PlayOneShot(clip, Instance.effectSource.volume);
            Destroy(tempEffectSource, clip.length);
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