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

        public void SliderSound(AudioClip clip)
        {

        }

        public void ChangeMusicVolume(float value)
        {
            PlayerPrefs.SetFloat("musVolumeKey", value);
            musicSource.volume = value;
        }

        public void ChangeSFXVolume(float value)
        {
            PlayerPrefs.SetFloat("sfxVolumeKey", value);
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

            // master
            if (!PlayerPrefs.HasKey("masterVolumeKey"))
            {
                PlayerPrefs.SetFloat("masterVolumeKey", 1.0f);
            }
            else
            {
                Instance.effectSource.volume = PlayerPrefs.GetFloat("masterVolumeKey");
            }
            
            PlayerPrefs.Save();
        }

    }
}