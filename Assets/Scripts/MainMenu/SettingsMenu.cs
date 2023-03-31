using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private InputScript inputScript;
    [SerializeField] private UnityEngine.UI.Button closeButton;
    
    void Update()
    {
        if (Input.GetKeyDown(inputScript.Pause)) {
            closeButton.onClick.Invoke();
        }
    }
}
