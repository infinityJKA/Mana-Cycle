using UnityEngine;

// the SaveDataManager object can be on the PlayerManager prefab along with the PlayerManager singleton there
public class SaveDataManager : MonoBehaviour {
    public static SaveDataManager instance {get; private set;}

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        var config = new FBPPConfig()
        {
            SaveFilePath = Application.persistentDataPath,
            SaveFileName = "manacycledata.sav",
            AutoSaveData = false,
            ScrambleSaveData = false,
        };

        Debug.Log("Using save data file at "+config.SaveFilePath+"/"+config.SaveFileName);
        FBPP.Start(config);

        PlayerManager.LoginGuest();
    }

    private void OnApplicationQuit() {
        FBPP.Save();
    }

    public void Save() {
        FBPP.Save();
    }
}