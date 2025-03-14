using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battle.Cycle;
using Cosmetics;

namespace Battle.Board {

    public class TileVisual : MonoBehaviour
    {
        // ---- Serialized
        [SerializeField] private Image bgImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image glowImage;
        [SerializeField] private Image mainDarkColorImage;

        private readonly Vector2 refIconSize = Vector2.one * 0.5f; // (0.5, 0.5)

        // ---- Constants
        /// <summary>Base reference color to use against when this tile is glowed</summary>
        private readonly Color baseGlowColor = new Color(0,0,0,0);
        /// <summary>Color to light this up when connected to ghost piece</summary>
        private readonly Color litGlowColor = new Color(1,1,1,0.4f);

        // ---- Annimation
        // Root transform to animate position of when  (set to either bg or maindarkcolor based on mana skin type)
        private Transform fallTransform;
        // Target position of this element, used for fall animations
        private Vector3 targetPosition;
        // Initial movement speed of this object when movement animated - distance in tiles per sec
        private const float initialSpeed = 40;
        // Acceleration of this piece when falling - distance in tiles per sec squared
        private const float acceleration = 55;
        // current speed
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

        private void Start() {
            if (glowImage == null) {
                Debug.LogWarning("No glow image on tile \""+name+"\"");
            }
        }

        // to be called if this is a ghost or cycle visual. animation/glow should not be used
        public void DisableVisualUpdates() {
            enabled = false;
        }

        private void Update() {
            if (moving) MovementUpdate();
            GlowUpdate();
        }

        private void MovementUpdate() {
            if (fallTransform.localPosition == targetPosition) {
                moving = false;
                if (onFallAnimComplete != null) onFallAnimComplete();
            } else {
                fallTransform.localPosition = Vector2.MoveTowards(fallTransform.localPosition, targetPosition, speed * Time.smoothDeltaTime);
                speed += acceleration * Time.smoothDeltaTime;
            }
        }

        private void GlowUpdate() {
            if (glowImage == null) return;

            if (Time.time - glowStartTime < glowDuration) {
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
            if (glow > 0) {
                if (!glowImage.enabled) glowImage.enabled = true;
                glowImage.color = Color.Lerp(baseGlowColor, litGlowColor, glow);
            } else {
                if (glowImage.enabled) glowImage.enabled = false;
            }
        }

        
        public void SetVisual(CosmeticValues cosmetics, int manaColor, bool isTrash = false)
        {
            // Debug.Log("Setting up visual");
            PaletteColor paletteColor;
            ManaIcon icon;
            if (manaColor >= 0) {
                icon = cosmetics.manaIcons[manaColor];
                paletteColor = cosmetics.paletteColors[manaColor];
            } else {
                icon = cosmetics.multicolorIcon;
                paletteColor = cosmetics.multicolorPaletteColor;
            }

            // if visual has an icon sprte, set up the image
            if (icon.iconSprite != null) {
                SetupIcon(icon, paletteColor, isTrash);
            } 
            // if not, simply use the material set up by BoardCosmeticAssets.
            else {
                fallTransform = mainDarkColorImage.transform;
                mainDarkColorImage.gameObject.SetActive(true);
                bgImage.gameObject.SetActive(false);
                iconImage.gameObject.SetActive(false);
                mainDarkColorImage.sprite = icon.bgSprite;
                if (isTrash) {
                    // multicolor trash doesnt exist (yet)
                    mainDarkColorImage.material = cosmetics.trashMaterials[manaColor];
                } else {
                    if (manaColor >= 0) {
                        mainDarkColorImage.material = cosmetics.materials[manaColor];
                    } else {
                        mainDarkColorImage.material = cosmetics.multicolorMaterial;
                    }
                }
            }

            if (glowImage) glowImage.sprite = icon.bgSprite;
        }

        /// <summary>
        /// For use in shop / cosmetics views outside of battle scene.
        /// creates its own unique material.
        /// </summary>
        public void SetVisualStandalone(PaletteColor paletteColor, Sprite iconSprite) {
            mainDarkColorImage.gameObject.SetActive(true);
            bgImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);

            // possible change: input a ManaIcon instead of a sprite, to allow previewing of the icons where icon and bg are seperate.
            Material mat = new Material(mainDarkColorImage.material);
            mainDarkColorImage.material = mat;
            mainDarkColorImage.sprite = iconSprite;
            mat.SetColor("_MainColor", paletteColor.mainColor);
            mat.SetColor("_DarkColor", paletteColor.darkColor);

        }

        public void SetupIcon(ManaIcon icon, PaletteColor paletteColor, bool isTrash = false) {
            Color mainColor = isTrash ? Color.Lerp(paletteColor.mainColor, CosmeticValues.darkenColor, 0.375f) : paletteColor.mainColor;
            Color darkColor = isTrash ? Color.Lerp(paletteColor.darkColor, CosmeticValues.darkenColor, 0.3f) : paletteColor.darkColor;

            fallTransform = bgImage.transform;
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
            mainDarkColorImage.sprite = board.cosmetics.obscuredSprite;
            SetColor(board.cosmetics.obscureColor);
            mainDarkColorImage.material = null;
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
            fallTransform.localPosition = from;
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
            if (bgImage.gameObject.activeSelf) bgImage.rectTransform.anchoredPosition = position;
            else mainDarkColorImage.rectTransform.anchoredPosition = position;
        }

        public void SetSizeDelta(Vector2 size) {
            if (bgImage.gameObject.activeSelf) bgImage.rectTransform.sizeDelta = size;
            else mainDarkColorImage.rectTransform.sizeDelta = size;
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

        public void SetDarkColorSprite(Sprite sprite){ 
            mainDarkColorImage.sprite = sprite;
        }
    }
}