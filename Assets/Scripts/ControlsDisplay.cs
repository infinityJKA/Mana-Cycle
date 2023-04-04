using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsDisplay : MonoBehaviour
{

    [SerializeField] private InputScript inputs;
    private TMPro.TextMeshProUGUI keyText;
    // Start is called before the first frame update
    void Start()
    {
        keyText = gameObject.GetComponent<TMPro.TextMeshProUGUI>();

        KeyCode[] keyList = new KeyCode[] {inputs.Up, inputs.Left, inputs.Down, inputs.Right, inputs.RotateLeft, inputs.RotateRight, inputs.Cast, inputs.Pause};

        keyText.text = "";
        foreach (KeyCode keyCode in keyList)
        {   
            keyText.text += Utils.KeySymbol(keyCode) + "\n";
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
