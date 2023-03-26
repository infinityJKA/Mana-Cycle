using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsDisplay : MonoBehaviour
{

    [SerializeField] private InputScript inputs;
    private TMPro.TextMeshProUGUI keyText;
    private KeyCode[] keyList;
    // Start is called before the first frame update
    void Start()
    {
        keyText = gameObject.GetComponent<TMPro.TextMeshProUGUI>();

        keyList = new KeyCode[] {inputs.Left, inputs.Right, inputs.Up, inputs.Down, inputs.RotateLeft, inputs.RotateRight, inputs.Cast, inputs.Pause};

        keyText.text = "";
        foreach (KeyCode k in keyList)
        {
            keyText.text += k.ToString() + "\n";
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
