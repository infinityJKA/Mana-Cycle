using UnityEngine;

public class SaveDataManager : MonoBehaviour {
    public static SaveDataManager instance {get; private set;}

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        SerializationManager.Load();
    }

    private void OnApplicationQuit() {
        SerializationManager.Save();
    }
}