using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to set various render settings at the beginning of the scene
public class RenderSettingsManager : MonoBehaviour
{
    [SerializeField] bool doFog;
    [SerializeField] Color fogColor;

    void Start()
    {
        RenderSettings.fog = doFog;
        RenderSettings.fogColor = fogColor;
    }
}
