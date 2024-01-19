using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPromptGroup : MonoBehaviour {
    [SerializeField] public List<InputPrompt> prompts;

    public void OnControlsChanged(PlayerInput playerInput) {
        Debug.Log("Controls changed! "+playerInput.devices[0].name);
        foreach (InputPrompt inputPrompt in prompts) {
            inputPrompt.OnControlsChanged(playerInput);
        }
    }
}