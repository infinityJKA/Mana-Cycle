using UnityEngine;

public class SaveDataManager : MonoBehaviour {
    public static SaveDataManager instance {get; private set;}

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        var config = new FBPPConfig()
        {
            SaveFileName = "save.data",
            AutoSaveData = false,
            ScrambleSaveData = false,
            EncryptionSecret = "my-secret",
            SaveFilePath = Application.persistentDataPath
        };

        FBPP.Start(config);
    }

    private void OnApplicationQuit() {
        
    }

    public void Save() {
        FBPP.Save();
    }
}