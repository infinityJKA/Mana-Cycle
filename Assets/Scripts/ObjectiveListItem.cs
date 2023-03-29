using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveListItem : MonoBehaviour
{
    // Objective this list item represents
    [SerializeField] public Objective objective;
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



    // Refresh this objective's text and color. Additionally, if complete, return true.
    public bool Refresh(GameBoard board) {
        textbox.text = objective.Status(board);
        bool completed = objective.IsCompleted(board);
        if (completed) textbox.color = Color.green;
        return completed;
    }
}
