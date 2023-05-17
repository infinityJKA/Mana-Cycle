using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

using Sound;

namespace SoloMode
{
    public class ArcadeWinScreen : MonoBehaviour
    {
        [SerializeField] Image portrait;
        private TransitionScript transitionHandler;
        void Start()
        {
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
            if (Storage.level != null) portrait.sprite = Storage.level.battler.sprite;
        }

        void Update()
        {
            if (Input.anyKeyDown)
            {
                transitionHandler.WipeToScene("MainMenu", reverse:true);
                SoundManager.Instance.SetBGM(SoundManager.Instance.mainMenuMusic);
            }
        }
    }
}