using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class WindowPannel : MonoBehaviour
{
    // buttons to select on open/close
    [SerializeField] public GameObject selectOnOpen;
    [SerializeField] private GameObject selectOnClose;

    // will also re-enable on close
    [SerializeField] private GameObject disableOnOpen;

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
        disableOnOpen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(selectOnOpen);
    }

    // close this window
    public void Close(float outTime = 0f)
    {
        gameObject.SetActive(false);
        disableOnOpen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(selectOnClose);
    }

    // public void SetOnOpen(GameObject g)
    // {
    //     selectOnOpen = g;
    // }

}
