using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shop : MonoBehaviour
{
    // items being sold
    public List<Item> shopItems;
    // prefab of item display
    [SerializeField] private GameObject itemDisplayPrefab;
    // vert layout for item displays
    [SerializeField] private GameObject itemDisplayParent;

    // Start is called before the first frame update
    void Start()
    {
        // add display prefabs for each item in shop inventory
        foreach (Item i in shopItems)
        {
            Instantiate(itemDisplayPrefab.transform, itemDisplayParent.transform);
        }

        // set selectOnOpen to first item in shop list
        GetComponent<WindowPannel>().selectOnOpen = (itemDisplayParent.transform.GetChild(0).gameObject);
        EventSystem.current.SetSelectedGameObject(GetComponent<WindowPannel>().selectOnOpen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
