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

    /** if the timer is currently running */
    private bool running = false;

    // The time the timer will end at.
    private float endTime;

    public void StartTimer() 
    {
        running = true;
        endTime = Time.time + duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (!running) return;

        float timeLeft = endTime - Time.time;

        if (timeLeft <= 0) {
            textbox.text = "0:00";
            player1.Defeat();
        } else {
            int seconds = (int)(timeLeft % 60);
            int minutes = (int)(timeLeft/60);
            textbox.text = minutes + ":" + (seconds+"").PadLeft(2, '0');
        }
    }
}
