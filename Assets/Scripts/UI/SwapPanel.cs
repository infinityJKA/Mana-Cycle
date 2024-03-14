using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.EventSystems;

public class SwapPanel : MonoBehaviour
{
    [SerializeField] public GameObject defaultSelectOnOpen;
    [SerializeField] private Animator anim;
    private bool animationBlocksNavigation = false;

    // set by swap panel manager
    [System.NonSerialized] public GameObject selectOnOpen;

    public void Show()
    {
        gameObject.SetActive(true);
        anim.ResetTrigger("Out");
        anim.SetTrigger("In");

        if (!animationBlocksNavigation) EventSystem.current.SetSelectedGameObject(selectOnOpen);
    }

    public void Hide()
    {
        anim.ResetTrigger("In");
        anim.SetTrigger("Out");
        EventSystem.current.SetSelectedGameObject(null);
    }

    // called on the last frame of hide animation with animation event
    public void AfterHide()
    {
        gameObject.SetActive(false);
    }

    public void AfterShow()
    {
        if (animationBlocksNavigation) EventSystem.current.SetSelectedGameObject(selectOnOpen);
    }

    public void SetAnimationBlocksNavigation(bool b)
    {
        animationBlocksNavigation = b;
    }
}
