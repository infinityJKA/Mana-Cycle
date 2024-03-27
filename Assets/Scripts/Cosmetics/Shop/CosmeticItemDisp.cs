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

        // set from shop tab script
        [System.NonSerialized] public SwapPanelManager panelManager;
        [System.NonSerialized] public PurchaseConfirmationPanel confirmationPanel;
        [System.NonSerialized] public SmoothScrollHandler scroller;

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
            // float scrollTarg = Utils.SnapScrollToChildPos(scroller.gameObject.GetComponent<ScrollRect>(), GetComponent<RectTransform>()).y - Utils.SnapScrollToChildPos(scroller.gameObject.GetComponent<ScrollRect>(), scroller.scrollTransform.GetChild(0).GetComponent<RectTransform>()).y;
            float scrollTarg = Utils.SnapScrollToChildPos(scroller.gameObject.GetComponent<ScrollRect>(), GetComponent<RectTransform>()).y - 40f;
            scroller.setTargetPos(scrollTarg);
        }

        public void Submitted()
        {
            confirmationPanel.item = item;
            confirmationPanel.Refresh();
            panelManager.OpenPanel(3);
        }
    }
}

