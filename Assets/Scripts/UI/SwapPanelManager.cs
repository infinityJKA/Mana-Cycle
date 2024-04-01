using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwapPanelManager : MonoBehaviour
{
    [SerializeField] SwapPanel[] panels;
    [SerializeField] int initialPanel = 0;

    // if true, selectables are selected/deselected based on in/out animation time
    // used to not cause unintentional behavior with automatic navigation
    [SerializeField] private bool animationBlocksNavigation = false;

    // used to re-select the last element you selected when returning to a menu
    // index matches indes in panels, the gameObject is what was selected last in that menu
    private GameObject[] lastSelected;
    public int currentPanel {get; private set;}

    void Start()
    {
        lastSelected = new GameObject[panels.Length];
        currentPanel = initialPanel;

        // show only initial panel
        for (int i = 0; i < panels.Length; i++)
        {
            SwapPanel panel = panels[i];
            panel.index = i;
            panel.gameObject.SetActive(false);
            panel.SetAnimationBlocksNavigation(animationBlocksNavigation);
        }

        OpenPanel(initialPanel);
    }

    public void OpenPanel(int index)
    {
        lastSelected[currentPanel] = EventSystem.current.currentSelectedGameObject;

        var selectOnOpen = (lastSelected[index] == null) ? panels[index].defaultSelectOnOpen : lastSelected[index];
        if (selectOnOpen != null) panels[index].selectOnOpen = selectOnOpen;

        panels[currentPanel].Hide();
        panels[index].Show();

        currentPanel = index;
    }
}
