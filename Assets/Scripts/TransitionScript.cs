using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class TransitionScript : MonoBehaviour
{
    public static TransitionScript instance {get; set;}

    [SerializeField] private GameObject wipeObj;
    [SerializeField] private Image wipeImg;
    public TransitionState transitionState {get; private set;} = TransitionState.None;

    public enum TransitionState {
        None,
        In,
        Waiting,
        Out
    }

    private static float inTime;
    private static float outTime;
    private static float timePassed;
    private static string gotoScene;
    private static bool wipingOut = false;
    private static bool inverted;
    // if false, will need to manually call ReadyToFadeOut()
    private static bool readyToFadeOut;
    // if current transition is loading scene additively
    private bool loadingAsync;
    private AsyncOperation sceneLoadOp;

    /// <summary>
    /// Action that will be invoked when transitioning out, and set to null after it is invoked.
    /// </summary>
    public Action onTransitionOut;

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // This should probably be moved to its own singleton but laziness & transitionscript is everywhere
        // should only be needed in editor when reload domain is disabled
        PlayerManager.LoginIfNotLoggedIn();
    }

    // Update is called once per frame
    void Update()
    {
        if (transitionState == TransitionState.In)
        {
            timePassed += Time.deltaTime;
            wipeImg.fillAmount = Mathf.Pow((timePassed + 0.1f) / inTime, 2);
            if (timePassed >= inTime){
                if (loadingAsync) {
                    transitionState = TransitionState.Waiting;
                    sceneLoadOp.allowSceneActivation = true;
                    StartCoroutine(TransitionOutWhenSceneLoaded());
                } else {
                    SceneManager.LoadScene(gotoScene);
                    WipeOut();
                }
                // when WipeOut is called here, it runs before the Start method and causes silly activity
                // WipeOut();
            }
        }

        else if (transitionState == TransitionState.Out && readyToFadeOut)
        {
            if (onTransitionOut != null) {
                onTransitionOut.Invoke();
                onTransitionOut = null;
            }

            timePassed += Time.deltaTime;
            wipeImg.fillAmount = Mathf.Pow(timePassed  / outTime, 2) * -1 + 1;
            if (wipeImg.fillAmount <= 0)
            {
                transitionState = TransitionState.None;
                wipingOut = false;
            }
        }
    }

    IEnumerator TransitionOutWhenSceneLoaded()
    {
        while (!sceneLoadOp.isDone)
        {
            yield return null;
        }
        WipeOut();
    }

    public void WipeToScene(string scene, float inTime=0.5f, float outTime=0.5f, bool reverse=false, bool autoFadeOut=true, bool asyncLoad = false)
    {
        // dont start a transition if one is already in progress
        if (transitionState == TransitionState.In) return;

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
        transitionState = TransitionState.In;
        wipingOut = false;
        gotoScene = scene;
        readyToFadeOut = autoFadeOut;
        loadingAsync = asyncLoad;

        if (asyncLoad) {
            sceneLoadOp = SceneManager.LoadSceneAsync(gotoScene);
            sceneLoadOp.allowSceneActivation = false;
        }
    }

    public void ReadyToFadeOut() {
        readyToFadeOut = true;
    }

    public void WipeOut()
    {
        if (wipingOut) return;
        wipingOut = true;

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
        transitionState = TransitionState.Out;
    }
}