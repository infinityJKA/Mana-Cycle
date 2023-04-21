using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace VersusMode {
    public class TimerManagerScript : MonoBehaviour
    {
        [SerializeField] private CharSelectScript p1Selector;
        [SerializeField] private CharSelectScript p2Selector;
        private TransitionScript transitionHandler;
        private double timer;
        private bool countdownStarted = false;
        private double maxTime = 1.0;

        // Start is called before the first frame update
        void Start()
        {
            transitionHandler = GameObject.Find("TransitionHandler").GetComponent<TransitionScript>();
        }

        // Update is called once per frame
        void Update()
        {
            // check if players are locked in
            if (p1Selector.GetLocked() && p2Selector.GetLocked())
            {
                // both players are ready'd
                if (!countdownStarted)
                {
                    countdownStarted = true;
                    timer = maxTime;
                }
            }
            else
            {
                // one or neither is ready'd
                if (countdownStarted){
                    countdownStarted = false;
                    timer = 0.0;
                }
            }

            // update timer, if applicable
            if (countdownStarted){
                timer -= Time.deltaTime;
                
            }

            // when time reached
            if (timer <= 0 && countdownStarted)
            {
                timer = 0;
                countdownStarted = false;

                Storage.battler1 = p1Selector.GetChoice();
                Storage.battler2 = p2Selector.GetChoice();
                Storage.isPlayer1 = p1Selector.GetPlayerType();
                Storage.isPlayer2 = p2Selector.GetPlayerType();
                Storage.level = null;
                Storage.gamemode = Storage.GameMode.Versus;

                transitionHandler.WipeToScene("ManaCycle");
            }
        }
    }
}
