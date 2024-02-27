using UnityEditor;
using UnityEngine;

public class ConditionalNetManagerToggle : MonoBehaviour {
    [SerializeField] private bool useSteamNetManager = true;

    [SerializeField] private GameObject pcManager, steamManager, webGLManager;

    private void Awake() {
        pcManager.SetActive(false);
        steamManager.SetActive(false);
        webGLManager.SetActive(false);
        
        #if UNITY_EDITOR // Editor
            var target = EditorUserBuildSettings.activeBuildTarget;
            if (target == BuildTarget.WebGL) {
                webGLManager.SetActive(true);
            } else {
                #if !DISABLESTEAMWORKS
                    if (useSteamNetManager) {
                        steamManager.SetActive(true);
                    } else {
                        pcManager.SetActive(true);
                    }
                #else
                    pcManager.SetActive(true);
                #endif
            }
        #else // Build
            #if UNITY_WEBGL
                webGLManager.SetActive(true);
            #elif !DISABLESTEAMWORKS
                if (useSteamNetManager) {
                    steamManager.SetActive(true);
                } else {
                    pcManager.SetActive(true);
                }
            #else
                pcManager.SetActive(true);
            #endif
        #endif
    }
}