using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMaterial : MonoBehaviour {
    [SerializeField] private Material m_renderMaterial = null;
    
    void Start()
    {
        if (m_renderMaterial == null)
        {
            Debug.LogError("no mat");
            return;
        }
        
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, m_renderMaterial);
    }
}