using UnityEngine;
using UnityEngine.UI;

public abstract class CustomPopup : MonoBehaviour {
    public Selectable firstSelected;

    // popup data struct is set by popupmanager
    // used by implementations of CustomPopup to access data like the title, desc, etc
    // and used to check that the popupUI really is currently showing this popup vua checking PopupUI.currentPopup equality
    public Popup popup {get; set;}

    public virtual void Init() {
        firstSelected.Select();
    }

    public void CloseThisPopup() {
        if (PopupUI.instance.currentPopup == popup) {
            PopupUI.instance.ClosePopup();
            Destroy(gameObject);
        }
    }
}