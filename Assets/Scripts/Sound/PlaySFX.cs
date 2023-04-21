using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sound {
    public class PlaySFX : MonoBehaviour
    {
        [SerializeField] private AudioClip _clip;
        void Start()
        {
            Sound.SoundManager.Instance.PlaySound(_clip);
        }
    }
}
