using System.Collections;
using System.Collections.Generic;
using SoloMode;
using UnityEngine;

public class NewPlayerNotif : MonoBehaviour
{
    void Awake()
    {
        // hide after clearing level 1
        gameObject.SetActive(!Level.IsLevelCleared("Level1"));
    }
}
