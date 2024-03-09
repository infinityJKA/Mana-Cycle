using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class TransitionScript : MonoBehaviour
{
    public static TransitionScript instance;

    [SerializeField] private GameObject wipeObj;
    [SerializeField] private Image wipeImg;
    public static string transitionState { get; private set; } = "none";
    private static float inTime;
    private static float outTime;
    private static float timePassed;
    private static string gotoScene;
    private static bool wipingOut = false;
    private static bool inverted;
    // if false, will need to manually call ReadyToFadeOut()
    private static bool readyToFadeOut;

    /// <summary>
    /// Action that will be invoked when transitioning out, and set to null after it is invoked.
    /// </summary>
    public Action onTransitionOut;

    private void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // DontDestroyOnLoad(this.gameObject);
        // wipeImg = wipeObj.GetComponent<Image>();
        if (wipingOut){
            wipingOut = false;
            WipeOut();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (transitionState == "in")
        {
            timePassed += Time.deltaTime;
            wipeImg.fillAmount = Mathf.Pow((timePassed + 0.1f) / inTime, 2);
            if (timePassed >= inTime){
                wipingOut = true;
                SceneManager.LoadScene(gotoScene);
                // when WipeOut is called here, it runs before the Start method and causes silly activity
                // WipeOut();

                if (onTransitionOut != null) onTransitionOut.Invoke();
                onTransitionOut = null;
            }
        }

        else if (transitionState == "out" && readyToFadeOut)
        {
            timePassed += Time.deltaTime;
            wipeImg.fillAmount = Mathf.Pow(timePassed  / outTime, 2) * -1 + 1;
            if (wipeImg.fillAmount <= 0)
            {
                transitionState = "none";
            }
        }
    }

    public void WipeToScene(string scene, float inTime=0.5f, float outTime=0.5f, bool reverse=false, bool autoFadeOut=true)
    {
        // dont start a transition if one is already in progress
        if (transitionState == "in") return;

        inverted = reverse;
        if (!inverted)
        {
            wipeImg.fillOrigin = (int) Image.OriginHorizontal.Right;
        }
        else
        {
            wipeImg.fillOrigin = (int) Image.OriginHorizontal.Left;
        }

        TransitionScript.inTime = inTime;
        TransitionScript.outTime = outTime;
        timePassed = 0;
        transitionState = "in";
        gotoScene = scene;
        readyToFadeOut = autoFadeOut;
    }

    public void ReadyToFadeOut() {
        readyToFadeOut = true;
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
        wipeImg.fillAmount = 1;
        transitionState = "out";
    }
}