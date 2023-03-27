using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;


public class TransitionScript : MonoBehaviour
{
    [SerializeField] private GameObject wipeObj;
    private Image wipeImg;
    private static string transitionState = "none";
    private static double inTime;
    private static double outTime;
    private static double timePassed;
    private static string gotoScene;
    private static bool wipingOut = false;
    private static bool inverted;

    // Start is called before the first frame update
    void Start()
    {
        // DontDestroyOnLoad(this.gameObject);
        wipeImg = wipeObj.GetComponent<Image>();
        if (wipingOut){
            wipingOut = false;
            WipeOut();
        }

    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if (transitionState == "in")
        {
            wipeImg.fillAmount = (float) Math.Pow((timePassed / inTime),2);
            if (timePassed >= inTime){
                wipingOut = true;
                SceneManager.LoadScene(gotoScene);
                // when WipeOut is called here, it runs before the Start method and causes silly activity
                // WipeOut();
            }
        }

        else if (transitionState == "out")
        {
            wipeImg.fillAmount = (float) Math.Pow((timePassed / outTime),2) * -1 + 1;
        }
        

    }

    // inverting direction of transition is WIP
    public void WipeToScene(string scene, double iT=0.5d, double oT=0.5d, bool i=false)
    {
        inverted = i;
        if (!inverted)
        {
            wipeImg.fillOrigin = (int) Image.OriginHorizontal.Right;
        }
        else
        {
            wipeImg.fillOrigin = (int) Image.OriginHorizontal.Left;
        }
       
        inTime = iT;
        outTime = oT;
        timePassed = 0d;
        transitionState = "in";
        gotoScene = scene;
    }

    public void WipeOut()
    {
        if (!inverted)
        {
            wipeImg.fillOrigin = (int) Image.OriginHorizontal.Left;
        }
        else
        {
            wipeImg.fillOrigin = (int) Image.OriginHorizontal.Right;
        }
        timePassed = 0d;
        transitionState = "out";
    }

}