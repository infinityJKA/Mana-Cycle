using Cosmetics;
using LootLocker.Requests;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cosmetics {
    public class CosmeticShop : MonoBehaviour {
        public static CosmeticShop instance;

        // when cosmetic menu is opened from another scene, it should modify this public var 
        // so that the correct scene is returned to
        public static string sceneOnBack = "MainMenu";


        [SerializeField] InputActionReference backAction;

        [SerializeField] CosmeticShopTab[] tabs;

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
        }

        /// <summary>
        /// Cause all tabs to add any shop items that may have been loaded.
        /// CatalogManager may call this via instance after items are finished loading.
        /// </summary>
        public void UpdateTabs() {
            foreach (var tab in tabs) {
                tab.MakeItems();
            }
        }

        // run on loot locker session response received
        public void OnConnected() {
            if (!useBackendCatalogs) return;

            // FOR TESTING: liist catalogs
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
                SidebarUI.instance.ToggleExpanded();
            } else {
                Back();
            }
        }

        public void Back() {
            CatalogManager.paletteColors.ClearAllEntries();
            CatalogManager.iconPacks.ClearAllEntries();

            TransitionScript.instance.WipeToScene(sceneOnBack, reverse: true);
        }
    }
}