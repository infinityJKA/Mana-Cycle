using UnityEngine;

namespace Battle {
    // Timer that ticks down. If time is reached, player loses
    public class Timer : MonoBehaviour
    {
        // Player 1 in the scene. they will lose once the timer is up
        [SerializeField] public Board.GameBoard player1;

        /** Duration of this timer in seconds. */
        [SerializeField] public int duration;

        /** Text to update timer text on */
        [SerializeField] private TMPro.TextMeshProUGUI textbox;

        /** Normal color shown */
        [SerializeField] private TMPro.TMP_ColorGradient greenGradient;

        /** Color shown when time is almost up */
        [SerializeField] private TMPro.TMP_ColorGradient redGradient;

        [SerializeField] private GameObject beepSFX;

        /** if the timer is currently running */
        public bool running {get; private set;} = false;

        // Time that the timer started at.
        private float startTime;

        // The time the timer will end at. 
        private float endTime;

        // show and count upwards in versus matches
        private bool countUpwards;

        private int lastTickTime;

        void Start() {
            textbox.enableVertexGradient = true;
            textbox.colorGradientPreset = greenGradient;
        }

        public void StartTimer() 
        {
            running = true;
            countUpwards = !player1.singlePlayer;
            startTime = Time.time;
            endTime = startTime + duration;

            Update();

            if(Storage.level && Storage.level.time == -1){
                textbox.text = "∞:∞";
                enabled = false;
                return;
            }
        }

        public void StopTimer() {
            running = false;
        }

        float timeFloat;

        // Update is called once per frame
        void Update()
        {
            if (!running) return;

            if (countUpwards) {
                timeFloat = Time.time - startTime;
            } else {
                timeFloat = SecondsRemaining();
            }

            if (Storage.level && timeFloat <= 20 && !Storage.level.survivalWin && Storage.level.time != -1) {
                textbox.colorGradientPreset = redGradient;
            }

            
            if (timeFloat <= 0) {
                textbox.text = "0:00";
                if (Storage.level && !Storage.level.survivalWin){
                    if (!player1.IsDefeated()) player1.Defeat();
                }
                else if(Storage.level && Storage.level.time != -1){
                    if (!player1.IsDefeated()) {
                        player1.RefreshObjectives();
                        if (!player1.IsWinner()) player1.Defeat();
                    }
                }
            } else{
                if(!Storage.level || Storage.level.time != -1){
                    textbox.text = Utils.FormatTime(timeFloat, showDecimal: true);
                    if (countUpwards) return;
                    int secondsLeftInt = Mathf.CeilToInt(timeFloat);
                    if (timeFloat < 5 && lastTickTime != secondsLeftInt) {
                        Instantiate(beepSFX);
                        lastTickTime = secondsLeftInt;
                    }
                }
            }
        }

        public float SecondsRemaining() {
            return endTime - Time.time;
        }

        public bool TimeUp() {
            return running && SecondsRemaining() <= 0;
        }
    }
}
