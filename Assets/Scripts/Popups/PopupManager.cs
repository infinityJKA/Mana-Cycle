using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour {

    public static PopupManager instance {get; private set;} = null;

    /// <summary>
    /// List of popups. The last item is the one currently being shown on top and acted on.
    /// When a popup is lcosed control is given to the next highest in the stack.
    /// </summary>
    private Stack<Popup> popupStack;

    public Popup currentPopup {get; private set;}
    public static bool showingPopup => instance.popupStack.Count > 0;


    [SerializeField] private Canvas canvas;
    [SerializeField] private BasicPopup basicPopupPrefab;


    GameObject previouslySelected = null;

    private void Awake() {
        if (instance) {
            Destroy(gameObject);
            return;
        }
        instance = this;

        popupStack = new Stack<Popup>();

        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F2)) {
            TestPopup();
        }
    }

    public void ShowPopup(Popup popup) {

        if (popupStack.Count == 0) {
            // store selection if there is no stack, meaning no popup is being shown
            previouslySelected = EventSystem.current.currentSelectedGameObject;
        }

        popupStack.Push(popup);
        currentPopup = popup;

        // will update contentsizefitters and such
        // LayoutRebuilder.ForceRebuildLayoutImmediate(popup.transform as RectTransform);
        // Canvas.ForceUpdateCanvases();
        // neither of these work :(

        popup.SelectFirst();
    }

    public void CurrentPopupClosed() {
        // remove uppermost from stack.
        popupStack.Pop();
        // give control to next highest popup in stack
        if (popupStack.TryPeek(out Popup nextPopup)) {
            currentPopup = nextPopup;
            nextPopup.SelectFirst();
        } else {
            // if stack empty return control to previously selected as stored in this class
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(previouslySelected);
        }
    }

    public void ShowBasicPopup(string title, string description, Action onConfirm = null) {
        BasicPopup basicPopup = Instantiate(basicPopupPrefab.gameObject, transform).GetComponent<BasicPopup>();

        basicPopup.title = title;
        basicPopup.description = description;
        basicPopup.confirmButtonText = "Close";
        basicPopup.onConfirm = onConfirm;

        ShowPopup(basicPopup);
    }

    [QFSW.QC.Command]
    public void TestPopup() {
        ShowBasicPopup("Test Popup", "hi there");
    }

    public void ShowError(Exception e) {
        Debug.LogError(e);
        ShowBasicPopup("Error", e.ToString());
    }

    public void ShowErrorMessage(string s) {
        ShowBasicPopup("Error", s);
    }
}