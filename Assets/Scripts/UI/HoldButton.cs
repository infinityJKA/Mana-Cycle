using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldButton : Selectable, ISubmitHandler /**, IPointerDownHandler, IPointerUpHandler */
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float totalPressTime;
    [SerializeField] private UnityEvent onHeld;
    [SerializeField] private InputActionReference submitAction;

    private bool disabled = false;

    private bool pressed;
    private float pressTimer;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!submitAction) return;
        submitAction.action.started += ctx => Press();
        submitAction.action.canceled += ctx => Unpress();
        disabled = false;
    }

    protected override void OnDisable() {
        base.OnDisable();
        submitAction.action.started -= ctx => Press();
        submitAction.action.canceled -= ctx => Unpress();
        disabled = true;
        Debug.Log(gameObject+" disabled");
    }

    private void Update() {
        if (pressed) {
            pressTimer += Time.unscaledDeltaTime;
            UpdateFill();

            if (pressTimer > totalPressTime) {
                onHeld.Invoke();
                Unpress();
            }
        }
    }

    private void UpdateFill() {
        float fillPercent = pressTimer / totalPressTime;
        fillImage.fillAmount = Math.Min(1f, fillPercent);
    }

    private void Press() {
        if (disabled || !enabled || !gameObject) return;
        if (EventSystem.current.currentSelectedGameObject != gameObject) return;
        Debug.Log(gameObject+" pressed");
        pressed = true;
    }

    private void Unpress() {
        if (disabled || !enabled || !gameObject) return;
        Debug.Log(gameObject+" unpressed");
        pressed = false;
        pressTimer = 0f;
        UpdateFill();
    }

    // public override void OnPointerDown(PointerEventData eventData)
    // {
    //     base.OnPointerDown(eventData);
    //     Press();
    //     Debug.Log(gameObject+" pressed");
    // }

    // public override void OnPointerUp(PointerEventData eventData)
    // {
    //     base.OnPointerDown(eventData);
    //     Unpress();
    //     Debug.Log(gameObject+" unpressed");
    // }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log(eventData.used);
    }
}