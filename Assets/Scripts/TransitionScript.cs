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
    private static float inTime;
    private static float outTime;
    private static float timePassed;
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
            wipeImg.fillAmount = Mathf.Pow((timePassed + 0.1f) / inTime, 2);
            if (timePassed >= inTime){
                wipingOut = true;
                SceneManager.LoadScene(gotoScene);
                // when WipeOut is called here, it runs before the Start method and causes silly activity
                // WipeOut();
            }
        }

        else if (transitionState == "out")
        {
            wipeImg.fillAmount = Mathf.Pow(timePassed  / outTime, 2) * -1 + 1;
        }
        

    }

    // inverting direction of transition is WIP
    public void WipeToScene(string scene, float iT=0.5f, float oT=0.5f, bool i=false)
    {

        // dont start a transition if one is already in progress
        if (transitionState == "in") return;

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
        timePassed = 0;
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
        timePassed = 0;
        transitionState = "out";
    }
}