using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class Popup : MonoBehaviour {
    [SerializeField] private GameObject firstSelected;

    [SerializeField] private bool cancellable = true;

    public abstract void OnShow();

    /// <returns>true if this popup can be closed or false if something is blocking it</returns>
    public abstract bool OnClose();

    public void SelectFirst() {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void Close() {
        if (!cancellable) return;

        if (PopupManager.instance.currentPopup == this) {
            bool shouldClose = OnClose();
            if (!shouldClose) return;
            
            Destroy(gameObject);
            PopupManager.instance.CurrentPopupClosed();
        } else {
            Debug.LogError("Trying to close popup that is not current popup. This would cause stack errors");
        }
    }
}