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

            if (!SaveDataManager.initialized) return;
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
            FBPP.SetFloat("musVolumeKey", value);
            musicSource.volume = value;
        }

        public void ChangeSFXVolume(float value)
        {
            FBPP.SetFloat("sfxVolumeKey", value);
        }

        public void Load()
        {
            // music
            if (!FBPP.HasKey("musVolumeKey"))
            {
                FBPP.SetFloat("musVolumeKey", 0.5f);
            }
            else
            {
                Instance.musicSource.volume = FBPP.GetFloat("musVolumeKey");
            }

            // sfx
            if (!FBPP.HasKey("sfxVolumeKey"))
            {
                FBPP.SetFloat("sfxVolumeKey", 0.5f);
            }
            else
            {
                Instance.effectSource.volume = FBPP.GetFloat("sfxVolumeKey");
            }

            // master
            if (!FBPP.HasKey("masterVolumeKey"))
            {
                FBPP.SetFloat("masterVolumeKey", 1.0f);
            }
            else
            {
                Instance.effectSource.volume = FBPP.GetFloat("masterVolumeKey");
            }
            
            FBPP.Save();
        }

    }
}