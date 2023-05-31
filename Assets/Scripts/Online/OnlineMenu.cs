using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Online {

    public class OnlineMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject connectionPanel;
        [SerializeField] private GameObject waitingPanel;

        // Start is called before the first frame update
        void Start()
        {
            ShowConnectionPanel();
        }

        public void HostButtonPressed() {
            NetworkManager.Singleton.StartHost();
            ShowWaitingPanel();
        }

        public void ClientButtonPressed() {
            NetworkManager.Singleton.StartClient();
            ShowWaitingPanel();
        }

        private void ShowConnectionPanel() {
            connectionPanel.SetActive(true);
            waitingPanel.SetActive(false);
        }

        private void ShowWaitingPanel() {
            connectionPanel.SetActive(false);
            waitingPanel.SetActive(true);
        }
    }
}
