using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Cosmetics
{
    public class PurchaseConfirmationPanel : MonoBehaviour
    {
        // item this panel can purchase
        [SerializeField] public CosmeticItem item;
        [SerializeField] private TextMeshProUGUI nameText;
        // [SerializeField] public TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image itemIcon;

        public void Refresh()
        {
            nameText.text = item.displayName;
            costText.text = "" + item.value;
            itemIcon.sprite = item.icon;
            itemIcon.color = item.iconColor;
        }

        public void PurchaseItem()
        {
            Debug.Log("PurchaseItem called with " + item.displayName);
        }
    }
}

