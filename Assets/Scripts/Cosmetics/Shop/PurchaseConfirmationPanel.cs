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

        private bool showing = false;

        // todo: change to ShopItem
        private CosmeticItem item;

        private void Awake() {
            instance = this;
        }

        public void ShowItem(CosmeticItem item)
        {
            this.item = item;
            nameText.text = item.displayName;
            costText.text = "" + item.value;
            itemIcon.sprite = item.icon;
            itemIcon.color = item.iconColor;
        }

        public void PurchaseItem()
        {
            if (!WalletManager.balancesLoaded) {
                Debug.LogWarning("Balances not loaded; purchase may not reflect actual owned coins");
            }

            // todo: use both coins/iridium based on shopItem values
            int cost = item.value;
            int balance = WalletManager.coins;

            if (balance >= cost) {
                // todo: integrate purchase with LootLocker
                WalletManager.coins -= cost;
                CosmeticAssets.current.AddItem(item);
                item.owned = true;
                Debug.Log("Purchased "+item.displayName+"!");
                CosmeticShop.instance.UpdateBalance();
                CosmeticShop.instance.UpdateTabDisplays();
                if (SidebarUI.instance) SidebarUI.instance.UpdateWalletDisplay();
                Close();
            } else {
                Debug.Log("Cannot afford item! EXTREMELY LOUD INCORRECT BUZZER.mp3");
                // instead of stopping player here, may want to show cost with red / transparent
                // on the actual item display in the list and buzz zound when tey try to select there
                // while not having enough currency
            }
        }

        public void Cancel() {
            Close();
        }

        public void Close() {
            swapPanelManager.OpenPanel(backToPanel);
        }
    }
}

