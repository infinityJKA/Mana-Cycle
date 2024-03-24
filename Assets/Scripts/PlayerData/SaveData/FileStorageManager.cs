using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// the SaveDataManager object can be on the PlayerManager prefab along with the PlayerManager singleton there
public class FileStorageManager {
    // right now the encrypt bool desont do anything but may work on this later.
    // binary formatting should be enough to prevent most tampering for now.
    public static void Save<T>(T data, string path, bool encrypt) {
        FileStream dataStream = new FileStream(path, FileMode.Create);

        BinaryFormatter converter = new BinaryFormatter();
        converter.Serialize(dataStream, data);

        dataStream.Close();
        
        Debug.Log("saved file "+path);
    }

    /// <returns>True if data exists and was loaded successfully</returns>
    // the data type (T) loaded must be a class, and have a new constructor for if the file does not exist, a brand new instance is created in its place
    public static T Load<T>(string path, bool decrypt) where T : class, new() {
        if (!File.Exists(path)) {
            Debug.Log(path+" not found, creating new "+typeof(T));
            return new T();
        }

        FileStream dataStream = new FileStream(path, FileMode.Open);

        BinaryFormatter converter = new BinaryFormatter();
        T data = converter.Deserialize(dataStream) as T;

        dataStream.Close();
        
        Debug.Log("loaded file "+path);
        return data;
    }
}