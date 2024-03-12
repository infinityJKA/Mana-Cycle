using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldInput : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float totalPressTime;
    [SerializeField] private UnityEvent onHeld;
    [SerializeField] private InputActionReference actionReference;

    [SerializeField] private Graphic[] colorWhenPressed;
    [SerializeField] private Color unpressedColor, pressedColor;

    private bool pressed;
    private float pressTimer;

    private void Start() {
        if (actionReference) {
            actionReference.action.started += ctx => Press();
            actionReference.action.canceled += ctx => Unpress();
        }   
    }

    private void Update() {
        if (pressed & pressTimer < totalPressTime) {
            pressTimer += Time.unscaledDeltaTime;
            UpdateFill();

            if (pressTimer >= totalPressTime) {
                onHeld.Invoke();
            }
        }
    }

    private void UpdateFill() {
        float fillPercent = pressTimer / totalPressTime;
        fillImage.fillAmount = Math.Min(1f, fillPercent);
    }

    public void Press() {
        pressed = true;

        foreach (var g in colorWhenPressed) {
            g.color = pressedColor;
        }
    }

    public void Unpress() {
        pressed = false;
        pressTimer = 0f;
        UpdateFill();

        foreach (var g in colorWhenPressed) {
            g.color = unpressedColor;
        }
    }
}