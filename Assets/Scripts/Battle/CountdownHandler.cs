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
        private double currentTime;
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

        // Start is called before the first frame update
        void Start()
        {
            countDownText = this.GetComponent<TMPro.TextMeshProUGUI>();
            currentTime = countDownTime + countDownDelay;
            // lastIntTime = GetIntTime(currentTime);
            lastIntTime = 4;

            countDownText.text = "";

            timer.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            // update time until "go" - negative if past
            currentTime -= Time.deltaTime;

            // If 0 reached and cycle has been activated
            if (cycleActivated) {
                // hide the go text if past go time
                if (currentTime <= -goTime)
                {
                    gameObject.SetActive(false);
                }
            } 
            
            // otherwise, still ticking down, cycle not activated yet
            else {
                // Tick if int time different than last frame
                if (GetIntTime(currentTime) != lastIntTime){
                    // if countdown has hit 0; init boards, go text and SFX
                    if (currentTime <= 0)
                    {
                        manaCycle.InitBoards();
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
                        countDownText.text = GetIntTime(currentTime).ToString();
                        SoundManager.Instance.PlaySound(tickSFX);
                    }
                }
                lastIntTime = GetIntTime(currentTime);
            }
        }

        int GetIntTime(double t)
        {
            return (int) Math.Ceiling(currentTime);
        }
    }

}