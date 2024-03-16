using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Battle.Cycle;
using Sound;

namespace Battle.Board {
    /// <summary>
    /// Stores all the information needed b ythe Battle scene to reflec tthe currently equipped cosmetics.
    /// Some Ability-specific assets are also stored here that may or may not change based on equipped cosmetics 
    /// </summary>
    public class CosmeticAssets : MonoBehaviour
    {
        // ONLY FOR USE IN SINGLEPLAYER
        // public static CosmeticAssets instance {get; private set;}
        // private void Start() {
        //     instance = this;
        // }

        [SerializeField] public GameObject ironSwordSFX, pyroBombSFX;

        [SerializeField] public Sprite ironSwordSprite, pyroBombSprite;

        [SerializeField] public GameObject goldMineObject;

        [SerializeField] public Sprite miniZmanSprite;
    }
}