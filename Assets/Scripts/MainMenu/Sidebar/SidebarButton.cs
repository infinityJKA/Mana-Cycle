using MainMenu;
using UnityEngine;
using UnityEngine.EventSystems;

public class SidebarButton : MonoBehaviour, ISelectHandler, IMoveHandler
{
    [SerializeField] private int index;
    [SerializeField] private bool mainButton;

    public void OnSelect(BaseEventData eventData)
    {
        if (!SidebarUI.instance) return;
        if (!SidebarUI.instance.expanded) {
            SidebarUI.instance.ToggleExpanded();
        }

        if (mainButton && index >= 0) Storage.lastSidebarItem = gameObject;
    }

    public void OnMove(AxisEventData eventData)
    {
        if (!SidebarUI.instance) return;
        // when cursoring left while in sidebar, go back to last selected button
        if (eventData.moveDir == MoveDirection.Left && SidebarUI.instance.expanded) {
            SidebarUI.instance.ToggleExpanded();
            SidebarUI.ReselectAfterClose();
        }
    }
}