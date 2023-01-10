using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour
{

    public static bool paused = false;
    // [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject InputObj_P1;
    [SerializeField] private GameObject InputObj_P2;
    private InputScript inputScript_P1;
    private InputScript inputScript_P2; 
    [SerializeField] private GameObject PauseUI;

    // Start is called before the first frame update
    void Start()
    {
        inputScript_P1 = InputObj_P1.GetComponent<InputScript>();
        inputScript_P2 = InputObj_P2.GetComponent<InputScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(inputScript_P1.Pause) || Input.GetKeyDown(inputScript_P2.Pause))
        {
            paused = (!paused);
            if (paused)
            { 
                Time.timeScale = 0f; 
                PauseUI.SetActive(true);
            }
            else 
            {
                 Time.timeScale = 1f; 
                 PauseUI.SetActive(false);
            }
        }
    }
}
