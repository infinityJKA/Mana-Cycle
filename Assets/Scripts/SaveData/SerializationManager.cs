using System;
using System.IO;
using UnityEngine;

public class SerializationManager {
    private static readonly string path = Path.Combine(Application.persistentDataPath, "savedata.save");
    public static bool dataLoaded => GameData.current != null;

    public static async void Save() {
        // don't save in webgl; will use playerprefs only
        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            Debug.LogWarning("Trying to save data in webgl");
            return;
        }

        try
        {
            string dataToSave = JsonUtility.ToJson(GameData.current);
            using (FileStream stream = File.Create(path)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    await writer.WriteAsync(dataToSave);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving game data: "+e);
        }
    }

    public static async void Load() {
        // don't save in webgl; will use playerprefs only
        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            Debug.LogWarning("Trying to save data in webgl");
            return;
        }

        if (!File.Exists(path)) {
            // if no file found, create a new empty game save data and use that.
            GameData.current = new GameData();
            return;
        }

        try
        {
            string dataToLoad;
            using (FileStream stream = File.Open(path, FileMode.Open)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    dataToLoad = await reader.ReadToEndAsync();
                }
            }

            GameData.current = JsonUtility.FromJson<GameData>(dataToLoad);
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading game data: "+e);
        }
    }
}