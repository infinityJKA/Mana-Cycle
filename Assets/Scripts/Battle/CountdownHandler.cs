using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Sound;

namespace Battle {
    public class CountdownHandler : MonoBehaviour
    {
        [SerializeField] private double countDownTime;
        [SerializeField] private double countDownDelay;
        [SerializeField] private double goTime;
        private double currentTimeUntilStart;
        private int lastIntTime;
        private TMPro.TextMeshProUGUI countDownText;
        [SerializeField] private Battle.Cycle.ManaCycle manaCycle;
        private bool cycleActivated = false;

        [SerializeField] private AudioClip tickSFX;
        [SerializeField] private AudioClip goSFX;

        // Player1, level retrieved from if in solo mode
        [SerializeField] private Board.GameBoard player1;

        // timer to start when countdown ends
        [SerializeField] private Timer timer;

        bool countdownStarted = false;

        // Start is called before the first frame update
        void Start()
        {
            countDownText = GetComponent<TMPro.TextMeshProUGUI>();

            countDownText.text = "";

            timer.gameObject.SetActive(false);

            // start countdown immediately if not online
            if (!Storage.online) StartTimer(countDownTime + countDownDelay);
            // in online, will wait for client ready message
        }

        public void StartTimer(double timeUntilStart) {
            Debug.Log("Countdown started - delay: "+timeUntilStart);
            currentTimeUntilStart = timeUntilStart;
            lastIntTime = Mathf.CeilToInt((float)currentTimeUntilStart);
            countdownStarted = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!countdownStarted) return;

            // update time until "go" - negative if past
            currentTimeUntilStart -= Time.deltaTime;

            // If 0 reached and cycle has been activated
            if (cycleActivated) {
                // hide the go text if past go time
                if (currentTimeUntilStart <= -goTime)
                {
                    gameObject.SetActive(false);
                }
            } 
            
            // otherwise, still ticking down, cycle not activated yet
            else {
                // Tick if int time different than last frame
                if (intTime != lastIntTime){
                    // if countdown has hit 0; init boards, go text and SFX
                    if (currentTimeUntilStart <= 0)
                    {
                        manaCycle.StartBattle();
                        if (player1.singlePlayer) {
                            timer.gameObject.SetActive(true);
                            timer.duration = player1.GetLevel().time;
                            timer.StartTimer();
                        }
                        countDownText.text = "GO!";
                        cycleActivated = true;
                        SoundManager.Instance.PlaySound(goSFX);
                        SoundManager.Instance.PlayBGM();
                    }

                    // if not reached 0 yet, tick; update text and play sound
                    else {
                        countDownText.text = intTime.ToString();
                        SoundManager.Instance.PlaySound(tickSFX);
                    }
                }
                lastIntTime = intTime;
            }
        }

        int intTime
        {
            get { return (int) Math.Ceiling(currentTimeUntilStart); }
        }
    }

}