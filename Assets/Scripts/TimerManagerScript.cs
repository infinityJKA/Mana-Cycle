using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TimerManagerScript : MonoBehaviour
{

    [SerializeField] private CharSelectScript p1Selector;
    [SerializeField] private CharSelectScript p2Selector;
    [SerializeField] private GameObject FadeObject;
    private Image fadeImg;
    private bool countdownStarted = false;
    private double timer = 0;
    private double maxTime = 1.5;
    // Start is called before the first frame update
    void Start()
    {
        fadeImg = FadeObject.GetComponent<Image>();
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
                fadeImg.color = new Color(0.0f, 0.0f, 0.0f, 0f);
            }
        }

        // update timer, if applicable
        if (countdownStarted){
            timer -= Time.deltaTime;
            fadeImg.color = new Color(0.0f, 0.0f, 0.0f, (float) ((maxTime - timer)/maxTime));
        }

        // when time reached
        if (timer <= 0 && countdownStarted)
        {
            timer = 0;
            Utils.setScene("ManaCycle");
        }

    }

}
