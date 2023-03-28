using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    // The text box contained in this objective
    [SerializeField] public TMPro.TextMeshProUGUI textbox;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /** function to determine if this objective is completed */
    public virtual bool IsCompleted(GameBoard board) {
        return false;
    }

    /** function to update the string displayed */
    public virtual void Refresh(GameBoard board) {
        textbox.text = "This is an objective";
    }
}
