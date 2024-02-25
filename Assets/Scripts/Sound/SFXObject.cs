using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sound
{
    public class SFXObject : MonoBehaviour
    {
        AudioSource audioSource;
        [SerializeField] AudioClip[] audioClips;
        [SerializeField] private Vector2 pitchRange;
        [SerializeField] public float volumeScale = 1f;
        [System.NonSerialized] public float pan = 0f;
        [System.NonSerialized] public float pitch = -1f;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClips[0];
            if (pitch < 0) audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            else audioSource.pitch = pitch;
            audioSource.volume = volumeScale * PlayerPrefs.GetFloat("sfxVolumeKey") * PlayerPrefs.GetFloat("masterVolumeKey");
            audioSource.panStereo = pan;

            audioSource.Play();
            Destroy(gameObject, audioSource.clip.length);
        }
    }

}
