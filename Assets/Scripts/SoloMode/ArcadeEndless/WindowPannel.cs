using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class WindowPannel : MonoBehaviour
{
    // buttons to select on open/close
    [SerializeField] private GameObject selectOnOpen;
    [SerializeField] private GameObject selectOnClose;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // open this window
    // TODO implement smooth transitions
    public void Open(float inTime = 0f)
    {
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(selectOnOpen);
    }

    // close this window
    public void Close(float outTime = 0f)
    {
        gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(selectOnClose);
    }

}
