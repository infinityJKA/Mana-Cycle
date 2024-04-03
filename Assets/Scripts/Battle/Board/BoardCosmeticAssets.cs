using UnityEngine;
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

        // called Multicolor, but really it's used to display any mana sprite that is represented by a negative ManaColor
        // such as ManaColor.Multicolor (-1) or ManaColor.None (-2).
        [Tooltip("Mana icon for misc abilities, such as the transparent overlay on geo crystals, should be a star or similar")] 
        [SerializeField] public ManaIcon multicolorIcon;

        [Tooltip("Palette color for misc abilities, typically white, color may change if white already in palette. that could get complicated tho")] 
        [SerializeField] public PaletteColor multicolorPaletteColor;

        [Tooltip("Sprite to use when tile color is obscured (via Zman's zblind).")]
        [SerializeField] public Sprite obscuredSprite;

        [Tooltip("Sprite color to use when obscured.")]
        [SerializeField] public Color obscureColor;

        // Base materials to use for tile visuals
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private Material mainDarkColorMaterial;

        // Controls waht tile glow looks like when blobs are cleared
        [SerializeField] public AnimationCurve glowAnimCurve;

        // ==== ability
        // sfx
        [SerializeField] public GameObject ironSwordSFX, pyroBombSFX;

        // sprites
        [SerializeField] public Sprite ironSwordSprite, pyroBombSprite, miniZmanSprite;

        // 3D meshes
        [SerializeField] public GameObject goldMineObject;

        // particle fx
        [SerializeField] public GameObject pyroBombParticleEffect;
        
        public static readonly Color lightenColor = new Color(1f, 1f, 0.9f);
        public static readonly Color darkenColor = new Color(0, 0, 0.025f);

         // Used for mana icons that DON'T have a defined seperate icon sprite - this means they are using the main+dark color shader.
        // Each color gets its own material for better performance.
        public Material[] materials {get; private set;}
        public Material multicolorMaterial;
        public Material[] trashMaterials {get; private set;}

        // used for ghost mana icons that DO have an icon sprite. each color gets its own material.
        public Material[] ghostBgMaterials {get; private set;}
        public Material multicolorGhostBgMaterial;
        public Material[] ghostIconMaterials {get; private set;}
        public Material multicolorGhostIconMaterial;

        // awake
        // may want to move this into some kind of Init()
        // because only want to load from cosmetic assets file is this isn't some kind of online opponent
        // so logic may be needed from an external class like ManaCycle.cs.
        private void Awake() {
            if (useCosmeticAssetsFile) {
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

            materials = new Material[ManaCycle.cycleUniqueColors];
            trashMaterials = new Material[ManaCycle.cycleUniqueColors];
            ghostBgMaterials = new Material[ManaCycle.cycleUniqueColors];
            ghostIconMaterials = new Material[ManaCycle.cycleUniqueColors];

            for (int i = 0; i < ManaCycle.cycleUniqueColors; i++)
            {
                // If does have an icon, the the ghost sprite will use the ghost material for outline; create one for this color.
                if (manaIcons[i].iconSprite) {
                    ghostBgMaterials[i] = new Material(ghostMaterial);
                    ghostBgMaterials[i].SetColor("_Color", paletteColors[i].mainColor);

                    ghostIconMaterials[i] = new Material(ghostMaterial);
                    ghostIconMaterials[i].SetColor("_Color", paletteColors[i].mainColor);
                    ghostIconMaterials[i].SetFloat("_Size", 1.2f);
                }
                // if no icon, BG sprite is using the mainDarkColor shader scheme, create a material for it.
                else {
                    materials[i] = new Material(mainDarkColorMaterial);
                    materials[i].SetColor("_MainColor", paletteColors[i].mainColor);
                    materials[i].SetColor("_DarkColor", paletteColors[i].darkColor);

                    trashMaterials[i] = new Material(materials[i]);
                    trashMaterials[i].SetColor("_MainColor", Color.Lerp(paletteColors[i].mainColor, darkenColor, 0.375f));
                    trashMaterials[i].SetColor("_DarkColor", Color.Lerp(paletteColors[i].darkColor, darkenColor, 0.3f));
                }
            }

            if (multicolorIcon.iconSprite) {
                multicolorGhostBgMaterial = new Material(ghostMaterial);
                multicolorGhostBgMaterial.SetColor("_Color", multicolorPaletteColor.mainColor);

                multicolorGhostIconMaterial = new Material(ghostMaterial);
                multicolorGhostIconMaterial.SetColor("_Color", multicolorPaletteColor.mainColor);
                multicolorGhostIconMaterial.SetFloat("_Size", 1.2f);
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