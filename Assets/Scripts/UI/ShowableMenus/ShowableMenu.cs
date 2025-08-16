using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class ShowableMenu : MonoBehaviour {
    // Call these from within implementations of ShowableMenus, within the ShowMenu, HideMenu, OpenMenu and CloseMenu methods
    public event Action onShow;
    public event Action onHide;
    public event Action onControlEnter;
    public event Action onControlExit;


    public bool showing {get; private set;}
    public bool controlling {get; private set;}
    private GameObject previousObject;


    /// <summary>
    /// Object to first select if non-null. if rememberObjectSelection is enabled and previousObject is non-null, that will be selected instead.
    /// </summary>
    [SerializeField] private GameObject firstSelected;

    /// <summary>
    /// If non-null, stop controlling this window if pressed.
    /// </summary>
    [SerializeField] private InputActionReference backAction;

    /// <summary>
    /// if true, pressing backAction will hide this in addition to exiting control for this menu.
    /// </summary>
    [SerializeField] public bool hideOnBackAction = false;

    /// <summary>
    /// If true, last selected object will be returned to when navigating off and back onto this menu.
    /// </summary>
    [SerializeField] public bool rememberObjectSelection = true;

    /// <summary>
    /// If true, all ui on this will become uninteractable while this menu is not controlled (unclickable and cant be navigated to using ui).
    /// </summary>
    [SerializeField] public bool uninteractableWhileNotControlled = false;

    [SerializeField] protected CanvasGroup uiCanvasGroup;

    protected virtual void OnEnable() {
        if (uiCanvasGroup && uninteractableWhileNotControlled) uiCanvasGroup.interactable = false;
    }

    protected virtual void OnDisable() {
        if (controlling) StopControllingMenu();
    }

    /// <summary>
    /// Called when back action is pressed while this is controlled.
    /// </summary>
    public void OnBackPressed(InputAction.CallbackContext ctx) {
        Debug.Log("Back button of "+gameObject+" pressed");
        StopControllingMenu();

        if (hideOnBackAction) HideMenu();
    }
    
    /// <summary>
    /// UI is shown on the screen in this function
    /// </summary>
    public void ShowMenu() {
        if (showing) return;
        Debug.Log("Showing menu "+gameObject);
        showing = true;
        onShow?.Invoke();
    }

    /// <summary>
    /// UI is hidden on the screen in this function.
    /// </summary>
    public void HideMenu() {
        if (!showing) return;

        // dont control while hidden
        if (controlling) StopControllingMenu();

        Debug.Log("Hiding menu "+gameObject);
        showing = false;

        onHide?.Invoke();
    }

    /// <summary>
    /// UIelement is selected and control is given to this menu in this function.
    /// </summary>
    public async Task ControlMenu() {
        if (controlling) return;

        // make sure menu is visible before it is controlled
        if (!showing) ShowMenu();

        // wait a frame to prevent double inputs (may change if delay becomes noticeable)
        await Awaitable.NextFrameAsync();

        if (controlling) return;

        Debug.Log("Controlling menu "+gameObject);
        controlling = true;
        if (uiCanvasGroup && uninteractableWhileNotControlled) uiCanvasGroup.interactable = true;
        if (backAction) {
            backAction.action.performed += OnBackPressed;
        }

        if (rememberObjectSelection && previousObject) {
            EventSystem.current.SetSelectedGameObject(previousObject);
        }
        else if (firstSelected) {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        onControlEnter?.Invoke();
    }

    /// <summary>
    /// UI control is taken away from this menu in this function.
    /// </summary>
    public void StopControllingMenu() {
        if (!controlling) return;

        if (rememberObjectSelection && EventSystem.current) previousObject = EventSystem.current.currentSelectedGameObject;

        Debug.Log("Exiting control of menu "+gameObject);
        controlling = false;
        if (uiCanvasGroup && uninteractableWhileNotControlled) uiCanvasGroup.interactable = false;
        if (backAction) {
            backAction.action.performed -= OnBackPressed;
        }
        onControlExit?.Invoke();

        if (backToMenu) {
            backToMenu.ControlMenu();
            backToMenu = null;
        }
    }

    /// <summary>
    /// If currently deferred to another menu, stop controlling those menus recursively, and then stop controlling this one.
    /// </summary>
    public void StopControllingMenuDeferred() {
        if (backToMenu) {
            backToMenu.StopControllingMenuDeferred();
            backToMenu = null;
        }

        StopControllingMenu();
    }

    private ShowableMenu backToMenu = null;

    /// <summary>
    /// Show and control this menu, but return control to passed menu when this menu is closed.
    /// </summary>
    public void ControlMenuDeferred(ShowableMenu menu) {
        if (!CheckIfCanDefer()) return;
        backToMenu = menu;
        backToMenu.StopControllingMenu();
        if (!showing) ShowMenu();
        ControlMenu();
    }

    public bool CheckIfCanDefer() {
        if (backToMenu) {
            Debug.LogError("Trying to defer "+this+" while it is already deferring to another menu: "+backToMenu);
            return false;
        }

        return true;
    }

    public void SetFirstObjectSelected(GameObject obj) {
        firstSelected = obj;
    }

    public void SetNextObjectSelected(GameObject obj) {
        if (rememberObjectSelection) {
            previousObject = obj;
        } else {
            firstSelected = obj;
        }
    }
}