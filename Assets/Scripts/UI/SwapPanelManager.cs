using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwapPanelManager : MonoBehaviour
{
    [SerializeField] SwapPanel[] panels;
    [SerializeField] int initialPanel = 0;

    // used to re-select the last element you selected when returning to a menu
    // index matches indes in panels, the gameObject is what was selected last in that menu
    private GameObject[] lastSelected;
    private int currentPanel;

    void Start()
    {
        lastSelected = new GameObject[panels.Length];
        currentPanel = initialPanel;

        // show only initial panel
        foreach (SwapPanel panel in panels)
        {
            panel.gameObject.SetActive(false);
        }

        panels[currentPanel].gameObject.SetActive(true);
    }

    public void OpenPanel(int index)
    {
        lastSelected[currentPanel] = EventSystem.current.currentSelectedGameObject;

        panels[currentPanel].Hide();
        panels[index].Show();

        panels[index].selectOnOpen = ( (lastSelected[index] == null) ? panels[index].defaultSelectOnOpen : lastSelected[index]);

        currentPanel = index;
    }
}
