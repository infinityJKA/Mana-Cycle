using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class InputPrompt : MonoBehaviour {
    [SerializeField] private Image image;

    [SerializeField] private Sprite keyboardSprite, keyboardPressedSprite, xboxSprite, xboxPressedSprite, playstationSprite, playstationPressedSprite;

    private Sprite unpressedSprite, pressedSprite;

    [SerializeField] private InputActionReference inputActionReference;

    [SerializeField] private bool listenForDeviceChange;

    [SerializeField] private InputPromptGlyphTable glyphTable;

    public void Press(InputAction.CallbackContext ctx) {
        image.sprite = pressedSprite;
    }

    public void Unpress(InputAction.CallbackContext ctx) {
        image.sprite = unpressedSprite;
    }

    private void OnEnable() {
        if (listenForDeviceChange) {
            InputSystem.onDeviceChange += OnDeviceChange;
            if (inputActionReference != null) {
                inputActionReference.action.started += Press;
                inputActionReference.action.canceled += Unpress;
            }
        }    
    }

    private void OnDisable() {
        if (listenForDeviceChange) {
            InputSystem.onDeviceChange -= OnDeviceChange;
            if (inputActionReference != null) {
                inputActionReference.action.started -= Press;
                inputActionReference.action.canceled -= Unpress;
            }
        }
    }

    public void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        // Debug.Log(device.name+": "+change);
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected) {
            // change inputs for all activeInputPrompts
        }
    }

    public void OnControlsChanged(PlayerInput playerInput) {
        // Debug.Log($"Input device changed for player {playerInput.playerIndex}: {playerInput.devices[0].name}");
        
        if (playerInput.currentControlScheme == "Keyboard&Mouse") {
            unpressedSprite = keyboardSprite;
            pressedSprite = keyboardPressedSprite;
        } else if (playerInput.currentControlScheme == "Gamepad") {
            InputDevice newDevice = playerInput.devices[0];
            if (playerInput.devices.Count < 1) {
                Debug.LogError("No devices on new playerinput & not keyboard...");
                return;
            }
            if (newDevice.description.product.ToLower().Contains("playstation")) {
                unpressedSprite = playstationSprite;
                pressedSprite = playstationPressedSprite;
            } else if (newDevice.description.product.ToLower().Contains("")) {
                unpressedSprite = xboxSprite;
                pressedSprite = xboxPressedSprite;
            }
        }

        if (!image) {
            image = GetComponentInChildren<Image>();
            if (image) {
                Debug.LogWarning("missing image ref on "+name+", using image from child");
            } else {
                Debug.LogError("missing image on "+name);
            }
        }

        if (inputActionReference != null) {
            image.sprite = inputActionReference.action.IsPressed() ? pressedSprite : unpressedSprite;
        } else {
            if (unpressedSprite != null) image.sprite = unpressedSprite;
            else if (pressedSprite!= null) image.sprite = pressedSprite;
            else Debug.LogError(gameObject+" does not have pressed OR unpressed sprite");
        }
    }
}