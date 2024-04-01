using Cosmetics;
using LootLocker.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmetics {
    public class CosmeticShop : MonoBehaviour {
        public static CosmeticShop instance;

        // when cosmetic menu is opened from another scene, it should modify this public var 
        // so that the correct scene is returned to
        public static string sceneOnBack = "MainMenu";

        // icon sprite to use for palette colors
        // system might change in the future
        [SerializeField] private Sprite _paletteColorIconSprite;
        public Sprite paletteColorIconSprite => _paletteColorIconSprite;

        [SerializeField] InputActionReference backAction;

        [SerializeField] CosmeticShopTab[] tabs;

        [SerializeField] private SwapPanelManager swapPanelManager;

        [SerializeField] private TMP_Text coinsLabel;

        // whether or not to use the online database. SHOULD BE TRUE FOR BUILD
        [SerializeField] private bool _useBackendCatalogs = true;
        public bool useBackendCatalogs => _useBackendCatalogs;

        private void Awake() {
            instance = this;
        }

        private void Start() {
            foreach (var tab in tabs) {
                tab.Initialize();
            }
            UpdateBalance();
        }

        /// <summary>
        /// Cause all tabs to add any shop items that may have been loaded.
        /// CatalogManager may call this via instance after items are finished loading.
        /// </summary>
        public void UpdateTabs() {
            foreach (var tab in tabs) {
                tab.MakeItems();
                tab.UpdateDisplays();
            }
        }

        public void UpdateTabDisplays() {
            foreach (var tab in tabs) {
                tab.UpdateDisplays();
            }
        }

        public void UpdateBalance() {
            coinsLabel.text = ""+WalletManager.coins;
        }

        // run on loot locker session response received
        public void OnConnected() {
            if (!useBackendCatalogs) return;

            // FOR TESTING: list catalogs
            LootLockerSDKManager.ListCatalogs((response) => {
                Debug.Log("CATALOGS RECEIVED!!!");

                foreach (var tab in tabs) {
                    tab.RunWhenConnected();
                }
            });
        }

        private void OnEnable() {
            backAction.action.performed += OnBack;
        }
        private void OnDisable() {
            backAction.action.performed -= OnBack;
        }

        public void OnBack(InputAction.CallbackContext ctx) {
            if (SidebarUI.instance && SidebarUI.instance.expanded) {
                ClosePurchaseConfirm();
                SidebarUI.instance.ToggleExpanded();
            } else if (swapPanelManager.currentPanel == 3) { // purchase confirmation
                ClosePurchaseConfirm();
            } else if (swapPanelManager.currentPanel != 0) {
                swapPanelManager.OpenPanel(0);
            } else {
                BackToPrevMenu();
            }
        }

        public void ClosePurchaseConfirm() {
            if (swapPanelManager.currentPanel == 3) PurchaseConfirmationPanel.instance.Cancel();
        }   

        public void BackToPrevMenu() {
            CatalogManager.paletteColors.ClearAllEntries();
            CatalogManager.iconPacks.ClearAllEntries();

            TransitionScript.instance.WipeToScene(sceneOnBack, reverse: true);
        }
    }
}