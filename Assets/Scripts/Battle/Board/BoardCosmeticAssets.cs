using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Battle.Cycle;
using Sound;
using Cosmetics;

namespace Battle.Board {
    /// <summary>
    /// Stores all the information needed b ythe Battle scene to reflec tthe currently equipped cosmetics.
    /// Some Ability-specific assets are also stored here that may or may not change based on equipped cosmetics 
    /// </summary>
    public class BoardCosmeticAssets : MonoBehaviour
    {
        // ONLY FOR USE IN SINGLEPLAYER
        // public static CosmeticAssets instance {get; private set;}
        // private void Start() {
        //     instance = this;
        // }

        [SerializeField] private Cosmetics.CosmeticDatabase cosmeticDatabase;

        // some of these probably won't change with cosmetics but im just using this to hold some ability assets together
        // mana palettes
        [Tooltip("Colors to tint the cycle images")]
        [SerializeField] public List<ManaPalette> manaColors;

        [Tooltip("Lit colors to tint cycle images for ghost blobs and clearing. Auto-generated if omitted. Should be a lighter & yellower version of the original color")]
        [SerializeField] public List<Color> litManaColors;

        [Tooltip("String representations of the mana colors")] 
        [SerializeField] public List<string> manaColorStrings;

        // mana palettes
        [Tooltip("Square sprites to use for mana in pieces and the cycle")]
        [SerializeField] public List<ManaIcon> manaVisuals;


        [Tooltip("Ghost tile verisons of manaSprites containing just the sahep outlines.")]
        [SerializeField] public List<Sprite> ghostManaSprites;

        // called Multicolor, but really it's used to display any mana sprite that is represented by a negative ManaColor
        // such as ManaColor.Multicolor (-1) or ManaColor.None (-2).
        [Tooltip("Mana sprite for misc abilities, such as the transparent overlay on geo crystals, should be a star or similar")] 
        [SerializeField] public Sprite multicolorManaSprite;

        [Tooltip("Ghost tile for the land position of an ability / multicolor tile.")]
        [SerializeField] public Sprite multicolorGhostManaSprite;

        // ==== ability
        // sfx
        [SerializeField] public GameObject ironSwordSFX, pyroBombSFX;

        // sprites
        [SerializeField] public Sprite ironSwordSprite, pyroBombSprite, miniZmanSprite;

        // 3D meshes
        [SerializeField] public GameObject goldMineObject;

        // particle fx
        [SerializeField] public GameObject pyroBombParticleEffect;
        

        private static readonly Color lightenColor = new Color(1f, 1f, 0.9f);
        private void Awake() {
            // get mana colors from player prefs
            // TODO: Use dynamic images/icons for mana sprites to use main and dark colors
            for (int i = 0; i < 5; i++)
            {
                manaColors[i] = cosmeticDatabase.colors[PlayerPrefs.GetInt("ManaColor" + i, i)];
                manaVisuals[i] = cosmeticDatabase.icons[PlayerPrefs.GetInt("ManaIcon" + i, i)];
            }
            
            while (litManaColors.Count < manaColors.Count) {
                Color originalColor = manaColors[litManaColors.Count].mainColor;
                Color lighterColor = Color.Lerp(originalColor, lightenColor, 0.75f);
                litManaColors.Add(lighterColor);
            }
        }


        /// <summary>
        /// Get the hex color (not manaColor int) of the mana at the specified index.
        /// </summary>
        public Color GetVisualManaColor(int index) {
            // for special flag colors such as Multicolor/Any/None with negative manacolor ID, just return color white.
            if (index < 0) return Color.white;

            return manaColors[index].mainColor;
        }

        public Color GetLitManaColor(int index) {
            if (index < 0) return Color.white;

            return litManaColors[index];
        }
    }
}