using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Utils : MonoBehaviour // please keep this as MonoBehavior to prevent component errors.
{
    public static void Shuffle<T>(List<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Random.Range(0, n + 1); 
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}