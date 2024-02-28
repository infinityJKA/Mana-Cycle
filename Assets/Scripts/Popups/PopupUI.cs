using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour {
    [SerializeField] private TMP_Text title, description, confirmButtonLabel;
    [SerializeField] private Button confirmButton;
    private Animator animator;

    private Popup currentPopup;
    public static bool showingPopup {get; private set;}

    private void Awake() {
        animator = GetComponent<Animator>();
        confirmButton.interactable = false;
    }

    public void Show(Popup popup) {
        currentPopup = popup;
        showingPopup = true;

        title.text = popup.title;
        description.text = popup.description;
        confirmButtonLabel.text = popup.confirmButtonText;

        confirmButton.interactable = true;
        confirmButton.Select();

        animator.SetBool("show", true);
    }

    public void OnConfirm() {
        if (currentPopup.onConfirm != null) currentPopup.onConfirm();
        showingPopup = false;
        confirmButton.interactable = false;
        animator.SetBool("show", false);
        
        PopupManager.instance.ShowNextPopup();
    }
}