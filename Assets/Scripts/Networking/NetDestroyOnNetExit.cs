using UnityEngine;
using UnityEngine.SceneManagement;

public class NetDestroyOnNetExit : MonoBehaviour {
        private void OnEnable() {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable() {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        // Destroy this object when leaving multiplayer gamemodes
        void OnSceneUnloaded(Scene scene) {
            if (!Storage.online) {
                Debug.Log("Destroying "+name);
                Destroy(gameObject);
            }
        }
}