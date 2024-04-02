using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class FileStorageManager {
    // right now the encrypt bool desont do anything but may work on this later.
    // binary formatting should be enough to prevent most tampering for now.
    public static void Save<T>(T data, string path, bool encrypt) {
        try {
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        } catch (Exception e) {
            Debug.LogError("Error saving file: "+e);
        }
        
        Debug.Log("saved file "+path);
    }

    /// <returns>True if data exists and was loaded successfully</returns>
    // the data type (T) loaded must be a class, and have a new constructor for if the file does not exist, a brand new instance is created in its place
    public static T Load<T>(string path, bool decrypt) where T : class, new() {
        if (!File.Exists(path)) {
            Debug.Log(path+" not found, creating new "+typeof(T));
            return new T();
        }

        try {
            string json = File.ReadAllText(path);
            Debug.Log("Loading "+path+"\n"+json);
            T data = JsonConvert.DeserializeObject<T>(json);
            return data;
        } catch (Exception e) {
            Debug.LogError("Error loading file: "+e);
            Debug.Log("returning new empty fallback instance");
            return new T();
        }
    }
}