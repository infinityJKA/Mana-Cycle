using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// tragic file
public class UIHandler : MonoBehaviour
{
    [SerializeField] InputScript[] inputScripts;
    EventSystem eventSystem;
    // Start is called before the first frame update
    void Start()
    {
        
    }
 
    private void Awake()
    {
        eventSystem = GetComponent<EventSystem>();
    }
 
    public void Move(MoveDirection direction)
    {
        AxisEventData data = new AxisEventData(EventSystem.current);
 
        data.moveDir = direction;
 
        data.selectedObject = EventSystem.current.currentSelectedGameObject;
 
        ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);
    }
 

    // Update is called once per frame
    void Update()
    {
        foreach (InputScript inputScript in inputScripts)
        {
            if (Input.GetKeyDown(inputScript.Cast))
            {
                Selectable button;
                // button = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
                if (EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() != null) button = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
                else button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                break;
            }

            if (Input.GetKeyDown(inputScript.Left))
            {
                Move(MoveDirection.Left);
                break;
            }

            if (Input.GetKeyDown(inputScript.Right))
            {
                Move(MoveDirection.Right);
                break;
            }

            if (Input.GetKeyDown(inputScript.Up))
            {
                Move(MoveDirection.Up);
                break;
            }

            if (Input.GetKeyDown(inputScript.Down))
            {
                Move(MoveDirection.Down);
                break;
            }
        }
    }
}
