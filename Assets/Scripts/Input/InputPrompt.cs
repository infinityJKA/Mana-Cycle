using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class InputPrompt : MonoBehaviour {
    [SerializeField] private InputUser user;

    [SerializeField] private Image keyboardImage, gamepadImage;

    private bool listening = false;

    private void OnEnable() {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    public void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        Debug.Log(device.name+": "+change);
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected) {
            // change inputs for all activeInputPrompts
        }
    }

    public void OnControlsChanged(PlayerInput playerInput) {
        Debug.Log($"Input device changed for player {playerInput.playerIndex}: {playerInput.devices[0].name}");
        InputDevice newDevice = playerInput.devices[0];
    }
}