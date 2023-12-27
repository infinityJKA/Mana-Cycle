using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenu : MonoBehaviour {
    private void Update() {
        if (Input.GetKeyDown(KeyCode.H)) {
            StartHost();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            StartClient();
        }
    }

    public void StartHost() {
        Debug.Log("Starting host!");
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient() {
        Debug.Log("Starting client!");
        NetworkManager.Singleton.StartClient();
    }
}