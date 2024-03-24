using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class Popup : MonoBehaviour {
    [SerializeField] private GameObject firstSelected;

    public void SelectFirst() {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void Close() {
        if (PopupManager.instance.currentPopup == this) {
            Destroy(gameObject);
            PopupManager.instance.CurrentPopupClosed();
        }
    }
}