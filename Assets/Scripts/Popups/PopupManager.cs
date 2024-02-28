using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopupManager : MonoBehaviour {

    public static PopupManager instance {get; private set;} = null;

    private Queue<Popup> popupQueue;

    public PopupUI popupUI;

    GameObject previouslySelected = null;

    private void Awake() {
        if (instance) {
            Destroy(gameObject);
            return;
        }
        instance = this;

        popupQueue = new Queue<Popup>();

        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F2)) {
            TestPopup();
        }
    }

    public void ShowPopup(Popup popup) {
        if (PopupUI.showingPopup) {
            popupQueue.Enqueue(popup);
        } else {
            previouslySelected = EventSystem.current.currentSelectedGameObject;
            popupUI.Show(popup);
            // StartCoroutine(ShowPopupNextFrame(popup)); // show next frame because ui is goofy
        }
    }

    // IEnumerator ShowPopupNextFrame(Popup popup) {
    //     yield return new WaitForEndOfFrame();
        
    // }

    public void ShowNextPopup() {
        popupUI.gameObject.SetActive(true);
        if (popupQueue.TryDequeue(out Popup nextPopup)) {
            popupUI.Show(nextPopup);
        } else {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(previouslySelected);
        }
    }

    [Command]
    public void ShowBasicPopup(string title, string description) {
        var popup = new Popup {
            title = title,
            description = description,
            confirmButtonText = "Close"
        };

        ShowPopup(popup);
    }

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

public struct Popup {
    public string title, description, confirmButtonText, cancelButtonText;
    public Action onConfirm, onCancel;
}