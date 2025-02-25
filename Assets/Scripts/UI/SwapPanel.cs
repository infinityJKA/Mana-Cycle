using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// using UnityEditor.Animations;
using UnityEngine.EventSystems;

public class SwapPanel : MonoBehaviour
{
    [SerializeField] public GameObject defaultSelectOnOpen;
    [SerializeField] private Animator anim;
    private bool animationBlocksNavigation = false;

    // set by SwapPanelManager in start
    public int index {get; set;} = -1;

    // set by swap panel manager
    [System.NonSerialized] public GameObject selectOnOpen;

    [SerializeField] public UnityEvent onOpened;
    [SerializeField] public UnityEvent onClosed;


    public bool showing {get; private set;} = false;

    public void Show()
    {
        showing = true;
        gameObject.SetActive(true);

        if (anim) {
            anim.ResetTrigger("Out");
            anim.SetTrigger("In");
        } else {
            gameObject.SetActive(true);
        }

        if (!animationBlocksNavigation && selectOnOpen) EventSystem.current.SetSelectedGameObject(selectOnOpen);

        onOpened.Invoke();
    }

    public void Hide()
    {
        showing = false;
        
        if (anim) {
            anim.ResetTrigger("In");
            anim.SetTrigger("Out");
        } else {
            gameObject.SetActive(false);
        }

        EventSystem.current.SetSelectedGameObject(null);

        onClosed.Invoke();
    }

    // called on the last frame of hide animation with animation event
    public void AfterHide()
    {
        gameObject.SetActive(false);
    }

    public void AfterShow()
    {
        if (animationBlocksNavigation && selectOnOpen) EventSystem.current.SetSelectedGameObject(selectOnOpen);
    }

    public void SetAnimationBlocksNavigation(bool b)
    {
        animationBlocksNavigation = b;
    }
}
