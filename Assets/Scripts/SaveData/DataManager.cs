using Cosmetics;
using SaveData;
using UnityEngine;

/// <summary>
/// Handles save data related tasks, particularly ones that require asset references / need to run on start
/// </summary>
public class DataManager : MonoBehaviour {
    public static DataManager instance {get; private set;}

    // All of these items will be added to player's inventory upon creating a new CosmeticAssets object.
    public CosmeticDatabase defaultAssets;

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnDestroy() {
        CosmeticAssets.Save();
    }
}