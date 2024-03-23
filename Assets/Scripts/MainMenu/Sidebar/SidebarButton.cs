using UnityEngine;
using UnityEngine.EventSystems;

public class SidebarButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        if (!SidebarUI.instance) return;
        if (!SidebarUI.instance.expanded) SidebarUI.instance.ToggleExpanded();
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        if (!SidebarUI.instance) return;
        if (SidebarUI.instance.expanded) SidebarUI.instance.ToggleExpanded();
    }
}