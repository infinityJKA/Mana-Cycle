using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.EventSystems;

public class SwapPanel : MonoBehaviour
{
    [SerializeField] public GameObject defaultSelectOnOpen;
    [SerializeField] private Animator anim;

    // set by swap panel manager
    [System.NonSerialized] public GameObject selectOnOpen;

    // TODO use animations
    public void Show()
    {
        gameObject.SetActive(true);
        anim.SetTrigger("In");
    }

    public void Hide()
    {
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
        EventSystem.current.SetSelectedGameObject(selectOnOpen);
    }
}
