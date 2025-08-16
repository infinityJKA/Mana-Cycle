using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ShowableMenu that simply activates and deactivates a gameobject to show and hide the menu.
/// </summary>
public class SimpleShowableMenu : ShowableMenu
{
    [SerializeField] private GameObject menuObject;
    [SerializeField] private bool dimWhileUncontrolled = true;
    [SerializeField] private float uncontrolledAlpha = 0.5f;

    protected override void OnEnable() {
        base.OnEnable();
        menuObject.SetActive(false);

        onShow += OnShow;
        onHide += OnHide;
        onControlEnter += OnControlEnter;
        onControlExit += OnControlExit;
    }

    protected override void OnDisable() {
        base.OnDisable();
        onShow -= OnShow;
        onHide -= OnHide;
        onControlEnter -= OnControlEnter;
        onControlExit -= OnControlExit;
    }

    public void OnShow() {
        menuObject.SetActive(true);
    }

    public void OnHide() {
        Debug.Log("hiding simple menu "+gameObject);
        menuObject.SetActive(false);
    }

    public void OnControlEnter()
    {
        if (dimWhileUncontrolled && uiCanvasGroup) uiCanvasGroup.alpha = 1;
    }

    public virtual void OnControlExit()
    {
        if (dimWhileUncontrolled && uiCanvasGroup) uiCanvasGroup.alpha = uncontrolledAlpha;
    }    
}