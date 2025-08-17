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
            UpdateMusicVolume();
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

        public void UpdateMusicVolume()
        {
            musicSource.volume = PlayerPrefs.GetFloat("musVolumeKey") * PlayerPrefs.GetFloat("masterVolumeKey");
        }

    }
}