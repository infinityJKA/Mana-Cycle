using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDisplay : MonoBehaviour
{
    // the item this display represents
    [SerializeField] public Item item;

    [SerializeField] private Image displayImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;

    // Start is called before the first frame update
    void Start()
    {
        displayImage.sprite = item.icon;
        nameText.text = item.itemName;
        costText.text = "" + item.cost;
    }

}
