using UnityEngine;
using UnityEngine.InputSystem;

public class InputTrackDevice : MonoBehaviour {
    [SerializeField] private InputPromptGroup inputPromptGroup;

    private void OnEnable() {
        GetComponent<PlayerInput>().onControlsChanged += inputPromptGroup.OnControlsChanged;
    }

    private void OnDisable() {
        GetComponent<PlayerInput>().onControlsChanged -= inputPromptGroup.OnControlsChanged;
    }
}