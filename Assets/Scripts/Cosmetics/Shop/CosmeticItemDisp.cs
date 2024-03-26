using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Cosmetics
{
    public class CosmeticItemDisp : MonoBehaviour
    {
        // the item this display is representing 
        [SerializeField] public CosmeticItem item;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] public TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image itemIcon;

        void Start()
        {
            nameText.text = item.displayName;
            costText.text = "" + item.value;
            itemIcon.sprite = item.icon;
            itemIcon.color = item.iconColor;
        }

        public void Selected()
        {
            descText.text = item.description;
        }
    }
}

