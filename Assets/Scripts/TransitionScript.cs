using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class TransitionScript : MonoBehaviour
{
    [SerializeField] private GameObject wipeObj;
    [SerializeField] private Image wipeImg;
    private static string transitionState = "none";
    private static float inTime;
    private static float outTime;
    private static float timePassed;
    private static string gotoScene;
    private static bool wipingOut = false;
    private static bool inverted;

    /// <summary>
    /// use to open a pre-loaded scene when transition is completed instead of a passed name.
    /// </summary>
    private static AsyncOperation sceneLoadOp;

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
        timePassed += Time.deltaTime;
        if (transitionState == "in")
        {
            wipeImg.fillAmount = Mathf.Pow((timePassed + 0.1f) / inTime, 2);
            if (timePassed >= inTime){
                wipingOut = true;

                if (sceneLoadOp != null) {
                    Debug.Log("allowing scene activation");
                    sceneLoadOp.allowSceneActivation = true;
                } else {
                    SceneManager.LoadScene(gotoScene);
                }
                
                // when WipeOut is called here, it runs before the Start method and causes silly activity
                // WipeOut();
            }
        }

        else if (transitionState == "out")
        {
            wipeImg.fillAmount = Mathf.Pow(timePassed  / outTime, 2) * -1 + 1;
        }
        

    }

    public void WipeToScene(string scene = "", AsyncOperation sceneLoadOp = null, float inTime=0.5f, float outTime=0.5f, bool reverse=false)
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
        TransitionScript.sceneLoadOp = sceneLoadOp;
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