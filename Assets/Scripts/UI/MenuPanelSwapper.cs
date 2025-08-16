using UnityEngine;

public class MenuPanelSwapper : MonoBehaviour {
    [SerializeField] private ShowableMenu[] _menus;
    public ShowableMenu[] menus => _menus;

    public void SetShownMenu(ShowableMenu menu) {
        foreach (var m in menus) {
            if (m) m.HideMenu();
        }

        if (menu) menu.ShowMenu();
    }

    public void HideAllMenus() {
        foreach (var m in menus) {
            if (m) m.HideMenu();
        }
    }
}