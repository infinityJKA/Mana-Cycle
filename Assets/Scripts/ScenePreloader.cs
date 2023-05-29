using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Preloads scenes into a dictionary of async handlers. Scnees can be pre-loaded, unloaded, and accessed from here.
/// </summary>
public class ScenePreloader
{
    static Dictionary<string, AsyncOperation> preloaded = new Dictionary<string, AsyncOperation>();

    /// <summary>
    /// Begin pre-loading a scene.
    /// </summary>
    /// <param name="sceneName">name of the scene to start pre-loading</param>
    /// <param name="gameObj">monobehaviour to attach the coroutine to</param>
    public static void Preload(string sceneName, MonoBehaviour mono) {
        // if already preloading, don't preload again
        if (preloaded.ContainsKey(sceneName)) return;

        Debug.Log("preloading "+sceneName);
        AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync(sceneName);
        mono.StartCoroutine(LoadLevelAsync(loadSceneOp, mono.gameObject));
    }

    static IEnumerator LoadLevelAsync(AsyncOperation loadSceneOp, GameObject tempObj) {
        loadSceneOp.allowSceneActivation = false;

        while (!loadSceneOp.isDone) {
            yield return null;
        }

        Debug.Log("finished loading "+tempObj.name);
        GameObject.Destroy(tempObj);
    }

    /// <summary>
    /// If a scene is no longer needed, unloads it asynchrously and stops the operation to load it.
    /// </summary>
    /// <param name="sceneName">name of the scene to unload</param>
    public static void Unload(string sceneName) {
        Debug.Log("unloading "+sceneName);
        SceneManager.UnloadSceneAsync(sceneName);
        preloaded.Remove(sceneName);
    }

    /// <summary>
    /// Immediately load the scene.
    /// If it is not pre-loaded, load it now synchrously.
    /// If it is pre-loaded, allow the pre-loaded async operation to complete the scene loading.
    /// </summary>
    /// <param name="sceneName"></param>
    public static void Load(string sceneName) {
        if (preloaded.ContainsKey(sceneName)) {
            Debug.Log("allowing "+sceneName+" to load");
            preloaded[sceneName].allowSceneActivation = true;
            preloaded.Remove(sceneName);
        } else {
            Debug.Log("synchrously loading "+sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}