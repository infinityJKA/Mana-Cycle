using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputTrackDevice : MonoBehaviour {
    [SerializeField] private InputPromptGroup inputPromptGroup;

    PlayerInput playerInput;

    private void OnEnable() {
        playerInput = GetComponent<PlayerInput>();
        inputPromptGroup.OnControlsChanged(playerInput);
        playerInput.onControlsChanged += inputPromptGroup.OnControlsChanged;
    }

    private void OnDisable() {
        playerInput.onControlsChanged -= inputPromptGroup.OnControlsChanged;
    }
}