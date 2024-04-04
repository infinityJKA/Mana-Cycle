using UnityEngine;
using UnityEngine.InputSystem;

// put on same object as input system
// disables the muse while not in use
public class MouseHandler : MonoBehaviour {
    Vector2 lastPosition;
    bool mouseEnabled;

    private void Start() {
        DisableMouse();
        
        // When position changes, re-enable mouse.
        InputSystem.onEvent +=
        (eventPtr, device) =>
        {
            if (device is Mouse mouse)
            {
                if (mouse.position.ReadValueFromEvent(eventPtr) != lastPosition) {
                    if (!mouseEnabled) {
                        InputSystem.EnableDevice(mouse);
                        Cursor.visible = true;
                    }
                }
            } else {
                DisableMouse();
            }
        };
    }

    void DisableMouse() {
        if (mouseEnabled) {
            // Disable device in frontend but not in backend.
            lastPosition = Mouse.current.position.ReadValue();
            InputSystem.DisableDevice(Mouse.current,
                keepSendingEvents: true);
            Cursor.visible = false;
            mouseEnabled = false;
        }   
    }
}