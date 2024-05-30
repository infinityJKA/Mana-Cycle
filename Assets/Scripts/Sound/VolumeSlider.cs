using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sound {
    public class VolumeSlider : MonoBehaviour
    {
        private SoundManager soundManager;
        [SerializeField] private Slider slider;
        [SerializeField] private string valueKey;
        [SerializeField] private GameObject changeSliderSFX;

        void Start()
        {
            // soundmanager is dontdestroyonload, so we need to re-find it on scene reload. otherwise it will detach from slider serialization
            soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            // Debug.Log(soundManager);

            // set slider positions
            if (PlayerPrefs.HasKey(valueKey))
            {
                slider.value = PlayerPrefs.GetFloat(valueKey);
            }
        }

        public void setMusVolume(float value)
        {
            if (soundManager == null) return;
            soundManager.ChangeMusicVolume(value);
        }

        public void setSFXVolume(float value)
        {
            if (soundManager == null) return;
            soundManager.ChangeSFXVolume(value);
            Instantiate(changeSliderSFX);
        }

        // public void PlaySound(AudioClip clip)
        // {
        //     soundManager.PlaySound(clip, 1f);
        // }
    }
}
