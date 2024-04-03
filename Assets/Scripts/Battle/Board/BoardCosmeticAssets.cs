using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Battle.Cycle;
using Sound;
using Cosmetics;
using SaveData;

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

        // if true, cosmetics grabbed form laoded file at battle start.
        [SerializeField] bool useCosmeticAssetsFile = true;

        // some of these probably won't change with cosmetics but im just using this to hold some ability assets together
        // mana palettes
        [Tooltip("Colors to tint the cycle images")]
        [SerializeField] public PaletteColor[] paletteColors;

        [Tooltip("Lit colors to tint cycle images for ghost blobs and clearing. Auto-generated if omitted. Should be a lighter & yellower version of the original color")]
        [SerializeField] public Color[] litManaColors;

        [Tooltip("String representations of the mana colors")] 
        [SerializeField] public string[] manaColorStrings;

        // mana palettes
        [Tooltip("Square sprites to use for mana in pieces and the cycle")]
        [SerializeField] public ManaIcon[] manaIcons;


        [Tooltip("Ghost tile verisons of manaSprites containing just the shape outlines.")]
        [SerializeField] public Sprite[] ghostManaSprites;

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

        // awake
        // may want to move this into some kind of Init()
        // because only want to load from cosmetic assets file is this isn't some kind of online opponent
        // so logic may be needed from an external class like ManaCycle.cs.
        private void Awake() {
            if (!useCosmeticAssetsFile) return;

            paletteColors = new PaletteColor[ManaCycle.cycleUniqueColors];
            manaIcons = new ManaIcon[ManaCycle.cycleUniqueColors];
            litManaColors = new Color[ManaCycle.cycleUniqueColors];

            // get mana colors from player prefs
            // TODO: Use dynamic images/icons for mana sprites to use main and dark colors
            for (int i = 0; i < ManaCycle.cycleUniqueColors; i++)
            {
                paletteColors[i] = CosmeticAssets.current.paletteColors[ CosmeticAssets.current.equippedPaletteColors[i] ];
                manaIcons[i] = CosmeticAssets.current.icons[ CosmeticAssets.current.equippedIcons[i] ];

                if (litManaColors[i] == Color.clear) {
                    litManaColors[i] = Color.Lerp(paletteColors[i].mainColor, lightenColor, 0.75f);
                }
            }
        }

        /// <summary>
        /// Get the visual main color (not manaColor int) of the mana at the specified index.
        /// </summary>
        public Color GetMainColor(int index) {
            // for special flag colors such as Multicolor/Any/None with negative manacolor ID, just return color white.
            if (index < 0) return Color.white;

            return paletteColors[index].mainColor;
        }

        public Color GetLitManaColor(int index) {
            if (index < 0) return Color.white;

            return litManaColors[index];
        }
    }
}