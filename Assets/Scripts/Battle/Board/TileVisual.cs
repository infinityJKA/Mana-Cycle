using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battle.Cycle;
using Cosmetics;

namespace Battle.Board {

    [RequireComponent(typeof(Tile))]
    public class TileVisual : MonoBehaviour
    {
        // ---- Serialized
        [SerializeField] private Image bgImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image glowImage;

        [SerializeField] private RawImage mainDarkColorImage;

        [SerializeField] private Vector2 refIconSize = Vector2.one * 0.5f; // (0.5, 0.5)

        // ---- Constants
        /// <summary>Base reference color to use against when this tile is glowed</summary>
        private readonly Color baseGlowColor = new Color(0,0,0,0);
        /// <summary>Color to light this up when connected to ghost piece</summary>
        private readonly Color litGlowColor = new Color(1,1,1,0.4f);

        // ---- Annimation
        // Target position of this element, used for fall animations
        private Vector3 targetPosition;
        // Initial movement speed of this object when movemnet animated - distance in tiles per sec
        private const float initialSpeed = 40;
        // Acceleration of this piece when falling
        private const float acceleration = 55; 
        private float speed;
        // If this piece is currently moving
        private bool moving = false;


        /// Variables for glow animation
        private float glow, glowStartTime, glowDuration, glowStart, glowTarget;

        /// <summary>While true, mana brightness will pulse in and out repeatedly</summary>
        public bool pulseGlow {get; set;}

        // Controls waht tile glow looks like when blobs are cleared
        private AnimationCurve glowAnimCurve;

        // Runs when this tile's fall animation is completed.
        // this probably belongs in Tile.cs and not TileVisual.cs... but nah
        public Action onFallAnimComplete {get; set;}

        // to be called if this is a ghost or cycle visual. animation/glow should not be used
        public void DisableVisualUpdates() {
            enabled = false;
        }

        private void Update() {
            if (moving) {
                if (bgImage.transform.localPosition == targetPosition) {
                    moving = false;
                    if (onFallAnimComplete != null) onFallAnimComplete();
                } else {
                    bgImage.transform.localPosition = Vector2.MoveTowards(bgImage.transform.localPosition, targetPosition, speed*Time.smoothDeltaTime);
                    speed += acceleration*Time.smoothDeltaTime;
                }
            }

            // Animate glow
            if (!glowImage) return;

            if (Time.time-glowStartTime < glowDuration) {
                // glow = Mathf.Lerp(glowStart, glowTarget, (Time.time-glowStartTime)/glowDuration);
                glow = Mathf.Lerp(glowStart, glowTarget, glowAnimCurve.Evaluate((Time.time-glowStartTime)/glowDuration));

                // Debug.Log("glow = "+glow);
            }
            // else, pulse glow if glowTarget is 0 (not animateing to a non-zero glow value), & not animating which previous condition stops
            else if (pulseGlow && glowTarget == 0) {
                glow = 0.45f + Mathf.PingPong(Time.time*1f, 0.2f);
            } 
            // otherwise, no glow if not animating or pulsing
            else {
                glow = glowTarget; // will be 0 unless animated before this
            }
            glowImage.color = Color.Lerp(baseGlowColor, litGlowColor, glow);
        }

        
        public void SetVisual(GameBoard board, int manaColor, bool isTrash = false)
        {
            // Debug.Log("Setting up visual");
            PaletteColor paletteColor;
            ManaIcon icon;
            if (manaColor >= 0) {
                icon = board.cosmetics.manaIcons[manaColor];
                paletteColor = board.cosmetics.paletteColors[manaColor];
            } else {
                icon = board.cosmetics.multicolorIcon;
                paletteColor = board.cosmetics.multicolorPaletteColor;
            }

            // if visual has an icon sprte, set up the image
            if (icon.iconSprite != null) {
                SetupIcon(icon, paletteColor, isTrash);
            } 
            // if not, simply use the material set up by BoardCosmeticAssets.
            else {
                mainDarkColorImage.gameObject.SetActive(true);
                bgImage.gameObject.SetActive(false);
                mainDarkColorImage.texture = icon.bgSprite.texture;
                if (isTrash) {
                    // multicolor trash doesnt exist (yet)
                    mainDarkColorImage.material = board.cosmetics.trashMaterials[manaColor];
                } else {
                    if (manaColor >= 0) {
                        mainDarkColorImage.material = board.cosmetics.materials[manaColor];
                    } else {
                        mainDarkColorImage.material = board.cosmetics.multicolorMaterial;
                    }
                }
            }

            if (glowImage) glowImage.sprite = icon.bgSprite;
        }

        public void SetupIcon(ManaIcon icon, PaletteColor paletteColor, bool isTrash = false) {
            Color mainColor = isTrash ? Color.Lerp(paletteColor.mainColor, BoardCosmeticAssets.darkenColor, 0.375f) : paletteColor.mainColor;
            Color darkColor = isTrash ? Color.Lerp(paletteColor.darkColor, BoardCosmeticAssets.darkenColor, 0.3f) : paletteColor.darkColor;

            bgImage.gameObject.SetActive(true);
            mainDarkColorImage.gameObject.SetActive(false);

            iconImage.sprite = icon.iconSprite;
            iconImage.color = darkColor;
            iconImage.rectTransform.anchoredPosition = icon.offset;
            iconImage.transform.eulerAngles = new Vector3(0f, 0f, icon.rotation);
            iconImage.rectTransform.sizeDelta = icon.scale * refIconSize;
            bgImage.sprite = icon.bgSprite;
            bgImage.color = mainColor;
            bgImage.material = null; // this should make it use default image UI shader??
        }

        public void SetObscuredVisual(GameBoard board)
        {
            bgImage.sprite = board.cosmetics.obscuredSprite;
            bgImage.color = board.cosmetics.obscureColor;
        }

        public void SetGhostVisual(GameBoard board, int manaColor)
        {
            DisableVisualUpdates(); // will not do glows for ghost tile

            PaletteColor paletteColor;
            ManaIcon icon;
            if (manaColor >= 0) {
                icon = board.cosmetics.manaIcons[manaColor];
                paletteColor = board.cosmetics.paletteColors[manaColor];
            } else {
                icon = board.cosmetics.multicolorIcon;
                paletteColor = board.cosmetics.multicolorPaletteColor;
            }

            mainDarkColorImage.gameObject.SetActive(false);
            bgImage.gameObject.SetActive(true);

            // if the icon contains a ghost sprite, use that. if not use inner glow & icon sprite&bg in the mana icon
            if (icon.ghostSprite) {
                bgImage.gameObject.SetActive(true);
                iconImage.gameObject.SetActive(false);
                mainDarkColorImage.gameObject.SetActive(false);
                bgImage.sprite = icon.ghostSprite;
                bgImage.color = paletteColor.mainColor;
                bgImage.material = null; // default image shader??
            } else {
                iconImage.sprite = icon.iconSprite;
                iconImage.color = paletteColor.darkColor;
                iconImage.material = board.cosmetics.ghostIconMaterials[manaColor];

                bgImage.sprite = icon.bgSprite;
                bgImage.color = paletteColor.mainColor;
                bgImage.material = board.cosmetics.ghostBgMaterials[manaColor];
            }
        }

        public void AnimateMovement(Vector2 from, Vector2 to) {
            bgImage.transform.localPosition = from;
            targetPosition = to;
            speed = initialSpeed;
            moving = true;
        }

        public void AnimateGlow(float target, float duration, AnimationCurve curve) {
            glowStart = glow;
            glowTarget = target;
            glowStartTime = Time.time;
            glowDuration = duration;
            glowAnimCurve = curve;
        }

        public void SetAnchoredPosition(Vector2 position) {
            bgImage.rectTransform.anchoredPosition = position;
        }

        public void SetSizeDelta(Vector2 size) {
            bgImage.rectTransform.sizeDelta = size;
        }

        public void SetSprite(Sprite sprite) {
            bgImage.sprite = sprite;
        }

        public void SetMaterial(Material material) {
            bgImage.material = material;
        }

        public void SetColor(Color color) {
            bgImage.color = color;
            mainDarkColorImage.color = color;
        }
    }
}