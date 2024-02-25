using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sound {
    public class PlaySFX : MonoBehaviour
    {
        [SerializeField] private GameObject _clip;
        void Start()
        {
            Instantiate(_clip);
        }

        public void PlaySound(GameObject sfx)
        {
            Instantiate(sfx);
        }
    }
}
