using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Basic popup with a title, some text, and a confirm button.
// Mainly used for error codes and brief but important notifications.
public class BasicPopup : Popup {
    [SerializeField] protected TMP_Text titleLabel, descriptionLabel, confirmButtonLabel;
    [SerializeField] protected Button confirmButton;

    public string title {
        set{ titleLabel.text = value; }
    }
    public string description{
        set{ descriptionLabel.text = value; }
    } 
    public string confirmButtonText {
        set{ confirmButtonLabel.text = value; }
    } 
    // public string cancelButtonText {
    //     set{  }
    // }

    public override void OnShow()
    {
        
    }

    public UnityEvent onConfirm;
    // public Action onCancel;

    public void OnConfirm() {
        onConfirm.Invoke();
        Close();
    }


}