using UnityEngine;

public class InputModuleRefresher : MonoBehaviour {
    // Idfk why this is needed but network stuffs messes this up
    private void Start() {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}