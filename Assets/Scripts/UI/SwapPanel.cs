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

    // set by swap panel manager
    [System.NonSerialized] public GameObject selectOnOpen;

    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;


    public bool showing {get; private set;} = false;

    public void Show()
    {
        showing = true;
        gameObject.SetActive(true);
        anim.ResetTrigger("Out");
        anim.SetTrigger("In");

        if (!animationBlocksNavigation && selectOnOpen) EventSystem.current.SetSelectedGameObject(selectOnOpen);

        onOpened.Invoke();
    }

    public void Hide()
    {
        showing = false;
        anim.ResetTrigger("In");
        anim.SetTrigger("Out");
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
