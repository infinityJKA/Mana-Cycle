using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ShowableMenu that simply activates and deactivates a gameobject to show and hide the menu.
/// </summary>
public class AnimatorShowableMenu : ShowableMenu
{
    [SerializeField] private Animator animator;
    [SerializeField] private string showingBoolName = "showing";
    [SerializeField] private string controllingBoolName = "controlling";
    [SerializeField] private GameObject firstSelected;

    protected override void OnEnable() {
        base.OnEnable();
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
        animator.SetBool(showingBoolName, true);
    }

    public void OnHide() {
        animator.SetBool(showingBoolName, false);
    }

    public void OnControlEnter()
    {
        animator.SetBool(controllingBoolName, true);
        if (firstSelected) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    public void OnControlExit()
    {
        animator.SetBool(controllingBoolName, false);
    }      
}