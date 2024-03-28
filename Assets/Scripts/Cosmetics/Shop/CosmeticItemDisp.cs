using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Cosmetics
{
    public class CosmeticItemDisp : MonoBehaviour, IMoveHandler
    {

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image itemIcon;

        // set via shop tab script
        private CosmeticItem item;
        private CosmeticShopTab tab;

        public void Init(CosmeticItem item, CosmeticShopTab tab) {
            this.item = item;
            this.tab = tab;
            nameText.text = item.displayName;
            costText.text = "" + item.value;
            itemIcon.sprite = item.icon;
            itemIcon.color = item.iconColor;
        }

        public void Selected()
        {
            if (!tab) return; // if tab not set probably wasnt initialized and might just be a preview disp
            tab.descriptionText.text = item.description;
            // float scrollTarg = Utils.SnapScrollToChildPos(scroller.gameObject.GetComponent<ScrollRect>(), GetComponent<RectTransform>()).y - Utils.SnapScrollToChildPos(scroller.gameObject.GetComponent<ScrollRect>(), scroller.scrollTransform.GetChild(0).GetComponent<RectTransform>()).y;
            float scrollTarg = Utils.SnapScrollToChildPos(tab.scroller.gameObject.GetComponent<ScrollRect>(), GetComponent<RectTransform>()).y - 40f;
            tab.scroller.setTargetPos(scrollTarg);
        }

        public void Submitted()
        {
            tab.confirmationPanel.item = item;
            tab.confirmationPanel.Refresh();
            tab.panelManager.OpenPanel(3);
        }

        public void OnMove(AxisEventData eventData)
        {
            tab.lastSelected = GetComponent<Selectable>();

            // to back button
            if (eventData.moveDir == MoveDirection.Right) {
                tab.backButton.Select();
            }

            // to next in list
            else if (eventData.moveDir == MoveDirection.Down) {
                int nextSibling = transform.GetSiblingIndex() + 1;
                if (nextSibling < transform.parent.childCount) {
                    transform.parent.GetChild(nextSibling).GetComponent<Selectable>().Select();
                } else {
                    // wrap
                    transform.parent.GetChild(0).GetComponent<Selectable>().Select();
                }
            }

            // to prev in list
            else if (eventData.moveDir == MoveDirection.Up) {
                int prevSibling = transform.GetSiblingIndex() - 1;
                if (prevSibling >= 0) {
                    transform.parent.GetChild(prevSibling).GetComponent<Selectable>().Select();
                } else {
                    // wrap
                    transform.parent.GetChild(transform.parent.childCount-1).GetComponent<Selectable>().Select();
                }
            }
        }
    }
}

