using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using QFSW.QC.Containers;
using SaveData;

namespace Cosmetics
{
    public class PurchaseConfirmationPanel : MonoBehaviour
    {
        public static PurchaseConfirmationPanel instance {get; private set;}

        // item this panel can purchase
        [SerializeField] private TextMeshProUGUI nameText;
        // [SerializeField] public TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private SwapPanelManager swapPanelManager;
        public int backToPanel {get; set;} = 1;

        // todo: change to ShopItem
        private CosmeticItemDisp disp;
        private ShopItem<CosmeticItem> shopItem => disp.shopItem;
        private CosmeticItem item => disp.shopItem.asset;

        private void Awake() {
            instance = this;
        }

        public void ShowItem(CosmeticItemDisp disp)
        {
            this.disp = disp;
            // item property => disp.shopIte.asset
            nameText.text = item.displayName;
            itemIcon.sprite = ;
            itemIcon.color = item.iconColor;
            costText.text = "" + shopItem.cost;
        }

        // when green Y button pressed
        public void PurchaseItem()
        {
            if (!WalletManager.balancesLoaded) {
                Debug.LogWarning("Balances not loaded; purchase may not reflect actual owned coins");
            }

            // todo: use both coins/iridium based on shopItem values
            int cost = disp.shopItem.cost;
            int balance = WalletManager.coins;

            if (balance >= cost) {
                if (CosmeticShop.instance.useBackendCatalogs) {
                    CatalogManager.PurchaseItem(disp.shopItem);
                } else {
                    PurchaseResponseReceived(success: true);
                }
            } else {
                Debug.Log("Cannot afford item! EXTREMELY LOUD INCORRECT BUZZER.mp3");
                // instead of stopping player here, may want to show cost with red / transparent
                // on the actual item display in the list and buzz zound when tey try to select there
                // while not having enough currency
            }
        }

        /// <summary>
        /// in online, call after purchase item request is finished.
        /// in offline testing, called immediately when button pressed
        /// </summary>
        /// <param name="success">if the item was purchased successfully</param>
        public void PurchaseResponseReceived(bool success) {
            if (!success) {
                // TODO: purchase error popup or somethin
                return;
            }

            // if success, update owned overlay to show that item is now bought
            disp.UpdateOwnedOverlay();
        }

        // when red N button pressed
        public void Cancel() {
            Close();
        }

        public void Close() {
            swapPanelManager.OpenPanel(backToPanel);
        }
    }
}

