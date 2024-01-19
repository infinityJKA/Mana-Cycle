using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerNotif : MonoBehaviour
{
    void Awake()
    {
        // hide after clearing level 1
        gameObject.SetActive(PlayerPrefs.GetInt("Level 1_Cleared", 0) == 0);
    }

    void Update()
    {
        
    }
}
