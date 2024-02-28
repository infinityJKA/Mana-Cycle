using UnityEditor;
using UnityEngine;

namespace Networking {
    public class NetworkManagerManager : MonoBehaviour {

        [SerializeField] private GameObject relayManager, steamManager, webGLManager;

        public enum NetworkManagerType {
            Relay, Steam, WebGL
        }
        [SerializeField] private NetworkManagerType _networkManagerType;
        public static NetworkManagerType networkManagerType {get; private set;}

        private void Awake() {
            networkManagerType = _networkManagerType;

            relayManager.SetActive(false);
            steamManager.SetActive(false);
            webGLManager.SetActive(false);
            
            #if UNITY_EDITOR // Editor
                var target = EditorUserBuildSettings.activeBuildTarget;
                if (target == BuildTarget.WebGL) {
                    webGLManager.SetActive(true);
                } else {
                    #if !DISABLESTEAMWORKS
                        if (networkManagerType == NetworkManagerType.Steam) {
                            steamManager.SetActive(true);
                        } else if (networkManagerType == NetworkManagerType.Relay) {
                            relayManager.SetActive(true);
                        }
                    #else
                        relayManager.SetActive(true);
                    #endif
                }
            #else // Build
                #if UNITY_WEBGL
                    webGLManager.SetActive(true);
                #elif !DISABLESTEAMWORKS
                    if (networkManagerType == NetworkManagerType.Steam) {
                        steamManager.SetActive(true);
                    } else if (networkManagerType == NetworkManagerType.Relay) {
                        relayManager.SetActive(true);
                    }
                #else
                    relayManager.SetActive(true);
                #endif
            #endif
        }
    }
}