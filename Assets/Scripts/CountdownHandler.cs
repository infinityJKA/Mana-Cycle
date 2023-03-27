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

    [SerializeField] private AudioClip tickSFX;
    [SerializeField] private AudioClip goSFX;

    // Start is called before the first frame update
    void Start()
    {
        countDownText = this.GetComponent<TMPro.TextMeshProUGUI>();
        currentTime = countDownTime + countDownDelay;
        lastIntTime = GetIntTime(currentTime);
    }

    // Update is called once per frame
    void Update()
    {
        currentTime -= Time.deltaTime;
        if (GetIntTime(currentTime) != lastIntTime){
            TimerTick();
        }
        lastIntTime = GetIntTime(currentTime);

        if (currentTime <= 0 && !cycleActivated)
        {
            manaCycle.InitBoards();
            countDownText.text = "GO!";
            cycleActivated = true;
            SoundManager.Instance.PlaySound(goSFX);
            
        }

        if (currentTime <= goTime*-1)
        {
            gameObject.SetActive(false);
        }

        
    }

    void TimerTick()
    {
        // called every time the number actually displayed by the timer text changes.
        countDownText.text = GetIntTime(currentTime).ToString();

        if (currentTime > 0){
            SoundManager.Instance.PlaySound(tickSFX);
        }
        

    }

    int GetIntTime(double t)
    {
        return (int) Math.Min(countDownTime,Math.Ceiling(currentTime));
    }
}
