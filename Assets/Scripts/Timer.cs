using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Timer that ticks down. If time is reached, player loses
public class Timer : MonoBehaviour
{
    // Player 1 in the scene. they will lose once the timer is up
    [SerializeField] public GameBoard player1;

    /** Duration of this timer in seconds. */
    [SerializeField] public int duration;

    /** Text to update timer text on */
    [SerializeField] private TMPro.TextMeshProUGUI textbox;

    /** Normal color shown */
    [SerializeField] private TMPro.TMP_ColorGradient greenGradient;

    /** Color shown when time is almost up */
    [SerializeField] private TMPro.TMP_ColorGradient redGradient;

    /** if the timer is currently running */
    private bool running = false;

    // The time the timer will end at.
    private float endTime;

    void Start() {
        textbox.enableVertexGradient = true;
        textbox.colorGradientPreset = greenGradient;
    }

    public void StartTimer() 
    {
        running = true;
        endTime = Time.time + duration;
    }

    public void StopTimer() {
        running = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!running) return;

        float timeLeft = endTime - Time.time;

        if (timeLeft <= 20 && !player1.level.survivalWin && player1.level.time != -1) {
            textbox.colorGradientPreset = redGradient;
        }

        if (timeLeft <= 0) {
            if(player1.level.time != -1){
                textbox.text = "0:00";
                if (!player1.level.survivalWin){
                    if (!player1.isDefeated()) player1.Defeat();
                }
                else if(player1.level.time != -1){
                    if (!player1.isDefeated()) player1.Win();
                }
            }
            else{
                textbox.text = "∞:∞";
            }
        } else{
            if(player1.level.time != -1){
                textbox.text = Utils.FormatTime(timeLeft);
            }
        }
    }
}
