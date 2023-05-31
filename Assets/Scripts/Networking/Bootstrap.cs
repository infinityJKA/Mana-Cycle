using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking {
    /// <summary>
    /// Script to initialize some things when the online multiplayer scene is loaded.
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SceneManager.LoadScene("ManaCycle");
        }
    }
}