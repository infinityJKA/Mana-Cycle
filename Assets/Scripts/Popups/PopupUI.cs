using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour {
    public static PopupUI instance {get; private set;}
    [SerializeField] private TMP_Text title, description, confirmButtonLabel;
    [SerializeField] private Button confirmButton;
    private Animator animator;

    public Popup currentPopup {get; private set;}
    public static bool showingPopup {get; private set;}
    private CustomPopup currentCustomPopup;

    [SerializeField] private GameObject defaultPopupObject;
    private GameObject customPopupObject = null;
    [SerializeField] private Transform customPopupTransform;

    private void Awake() {
        instance = this;
        animator = GetComponent<Animator>();
        confirmButton.interactable = false;
    }

    public void Show(Popup popup) {
        currentPopup = popup;
        showingPopup = true;

        if (popup.customWindowPrefab != null) {
            if (customPopupObject != null) Destroy(customPopupObject);
            defaultPopupObject.SetActive(false);
            customPopupObject = Instantiate(popup.customWindowPrefab.gameObject, customPopupTransform);
            currentCustomPopup = customPopupObject.GetComponent<CustomPopup>();
            currentCustomPopup.popup = popup;
            currentCustomPopup.Init();
        } else {
            defaultPopupObject.SetActive(true);
            title.text = popup.title;
            description.text = popup.description;
            confirmButtonLabel.text = popup.confirmButtonText;
            confirmButton.interactable = true;
            confirmButton.Select();
        }

        animator.SetBool("show", true);
    }

    public void OnConfirm() {
        if (currentPopup.onConfirm != null) currentPopup.onConfirm();
        confirmButton.interactable = false;
        ClosePopup();
    }

    public void ClosePopup() {
        showingPopup = false;
        animator.SetBool("show", false);
        
        if (!PopupManager.instance) return;
        StartCoroutine(ShowNextPopupAfterDelay());
    }

    IEnumerator ShowNextPopupAfterDelay() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        PopupManager.instance.ShowNextPopup();
    }
}