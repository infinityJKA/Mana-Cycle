using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputTrackDevice : MonoBehaviour {
    PlayerInput playerInput;

    private void OnEnable() {
        playerInput = GetComponent<PlayerInput>();
        UpdateInputPrompts(playerInput);
        playerInput.onControlsChanged += UpdateInputPrompts;
        Debug.Log(gameObject+" listening for device changes");
    }

    private void OnDisable() {
        playerInput.onControlsChanged -= UpdateInputPrompts;
    }

    public void UpdateInputPrompts(PlayerInput playerInput) {
        foreach (InputPrompt inputPrompt in transform.GetComponentsInChildren<InputPrompt>()) {
            Debug.Log("change control call on "+inputPrompt.gameObject);
            inputPrompt.OnControlsChanged(playerInput);
        }
    }
}