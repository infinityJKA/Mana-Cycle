using UnityEngine;

// the SaveDataManager object can be on the PlayerManager prefab along with the PlayerManager singleton there
public class SaveDataManager {
    public static bool initialized {get; private set;} = false;

    static SaveDataManager() {
        var config = new FBPPConfig()
        {
            SaveFilePath = Application.persistentDataPath,
            SaveFileName = "manacycledata.sav",
            AutoSaveData = false,
            ScrambleSaveData = false,
        };

        Debug.Log("Using save data file at "+config.SaveFilePath+"/"+config.SaveFileName);
        FBPP.Start(config);
        initialized = true;
    }

    private void OnApplicationQuit() {
        // cache some player information, so that next time they are offline/connecting on next game start, can see their username/other info
        FBPP.SetString("playerUsername", PlayerManager.playerUsername);

        FBPP.Save();
    }

    public void Save() {
        FBPP.Save();
    }
}