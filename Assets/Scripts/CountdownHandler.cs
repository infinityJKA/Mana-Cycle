using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CountdownHandler : MonoBehaviour
{
    [SerializeField] private double countDownTime;
    [SerializeField] private double countDownDelay;
    [SerializeField] private double goTime;
    private double currentTime;
    private int lastIntTime;
    private TMPro.TextMeshProUGUI countDownText;
    [SerializeField] private ManaCycle manaCycle;
    private bool cycleActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        countDownText = this.GetComponent<TMPro.TextMeshProUGUI>();
        currentTime = countDownTime + countDownDelay;
        lastIntTime = getIntTime(currentTime);
    }

    // Update is called once per frame
    void Update()
    {
        currentTime -= Time.deltaTime;
        if (getIntTime(currentTime) != lastIntTime){
            TimerTick();
        }
        lastIntTime = getIntTime(currentTime);

        if (currentTime <= 0 && !cycleActivated)
        {
            manaCycle.InitBoards();
            countDownText.text = "GO!";
            cycleActivated = true;
            
        }

        if (currentTime <= goTime*-1)
        {
            gameObject.SetActive(false);
        }

        
    }

    void TimerTick()
    {
        // called every time the number actually displayed by the timer text changes.
        countDownText.text = getIntTime(currentTime).ToString();

    }

    int getIntTime(double t)
    {
        return (int) Math.Min(countDownTime,Math.Ceiling(currentTime));
    }
}
