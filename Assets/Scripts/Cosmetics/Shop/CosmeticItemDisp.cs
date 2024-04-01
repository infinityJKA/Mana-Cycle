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
        [SerializeField] private GameObject ownedOverlay;

        // set via shop tab script
        public ShopItem<CosmeticItem> shopItem {get; private set;}
        private CosmeticShopTab tab;

        public void Init(ShopItem<CosmeticItem> shopItem, CosmeticShopTab tab) {
            this.shopItem = shopItem;
            this.tab = tab;
            nameText.text = shopItem.asset.displayName;
            itemIcon.sprite = shopItem.asset.icon;
            itemIcon.color = shopItem.asset.iconColor;
            costText.text = "" + shopItem.cost;
            UpdateOwnedOverlay();
        }

        public void UpdateOwnedOverlay() {
            ownedOverlay.SetActive(shopItem.owned);
        }

        public void Selected()
        {
            if (!tab) return; // if tab not set probably wasnt initialized and might just be a preview disp
            tab.lastSelected = GetComponent<Selectable>();
            tab.descriptionText.text = shopItem.asset.description;
            tab.scroller.SnapTo(GetComponent<RectTransform>());
        }

        public void Submitted()
        {
            if (shopItem.owned) {
                // TODO: buzzer shound/disp shake to indicate cant buy cause already owned
                return;
            }
            tab.confirmationPanel.ShowItem(this);
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

