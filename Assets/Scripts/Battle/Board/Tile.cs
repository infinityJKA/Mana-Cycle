using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battle.Cycle;

namespace Battle.Board {
    public class Tile : MonoBehaviour
    {
        // If within a piece, the row and column relative to that piece, before rotation orientation.
        // If placed on the board, the row and column of this tile on the grid.
        public int row;
        public int col;

        // Mana color int representation value for this tile (changed from ManaColor enum -> int)
        public int manaColor { get; private set; }

        // Seperate image object attached to this
        public Image image;

        // Target position of this element, used for fall animations
        private Vector3 targetPosition;

        // Initial movement speed of this object when movemnet animated - distance in tiles per sec
        public float initialSpeed = 0;
        private float speed;
        // Acceleration of this piece when falling
        public float acceleration = 10; 
        // If this piece is currently moving
        private bool moving = false;

        // Point multiplier when this tile is cleared. May be modified by abilities.
        public float pointMultiplier = 1f;

        // Runs when this tile's fall animation is completed.
        public Action onFallAnimComplete;

        // Runs right before this tile is cleared. If part of a blob, that blob is passed.
        public Action<GameBoard.Blob> beforeClear;

        // If this is a trash tile - which damages in set intervals
        public bool trashTile { get; private set; }

        // Whether or not this tile obscures mana colors around it. (zman)
        public bool obscuresColor { get; private set; }

        // Whether or not this tile's color is currently obscured.
        public bool obscured { get; private set; }

        // Sprite to use when this tile is obscured.
        [SerializeField] private Sprite obscuredSprite;

        // Sprite color to use while obscured.
        [SerializeField] private Color obscureColor;

        // Fragile tiles are only cleared when an adjacent blob is cleared.
        public bool fragile { get; private set; }

        // Duration left before this tile destroys itself - ticks down if set to above 0
        public float lifespan {get; private set;}

        // If gravity should pull this tile down.
        public bool doGravity { get; private set; } = true;

        /// <summary>Base reference color to use against when this tile is glowed</summary>
        private Color baseColor;
        /// <summary>Color to light this up when connected to ghost piece</summary>
        private Color litColor;
        /// <summary>While true, mana brightness will pulse in and out repeatedly</summary>
        public bool pulseGlow;
        /// <summary>internal bool used by GameBoard when lighting connected tiles</summary>
        public bool connectedToGhostPiece;

        /// Variables for glow animation
        private float glow, glowStartTime, glowDuration, glowStart, glowTarget;

        // Animation curve that controls what the tile glow looks like
        [SerializeField] private AnimationCurve glowAnimCurve;
        

        public void SetManaColor(int manaColor, GameBoard board, bool setVisualColor = true, bool setSprite = true, bool ghost = false)
        {
            this.manaColor = manaColor;

            // Get image and set color from the list in this scene's cycle
            if (setVisualColor) {
                baseColor = board.cosmetics.GetVisualManaColor( manaColor );
                litColor = board.cosmetics.GetLitManaColor( manaColor );
            }
            if (setSprite && ManaCycle.instance.usingSprites) {
                if (ghost) {
                    // if below 0, use multicolor ghost sprite
                    image.sprite = manaColor < 0 ? board.cosmetics.multicolorGhostManaSprite : board.cosmetics.ghostManaSprites[ manaColor ];
                    baseColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.4f);
                    image.GetComponent<UnityEngine.UI.Outline>().enabled = true;
                    // image.GetComponent<UnityEngine.UI.Outline>().effectColor = Color.Lerp(image.color, Color.white, 0.4f);
                    image.GetComponent<UnityEngine.UI.Outline>().effectColor = baseColor;
                } else {
                    // if below 0, use multicolor sprite
                    image.sprite = manaColor < 0 ? board.cosmetics.multicolorManaSprite : board.cosmetics.manaSprites[ manaColor ];
                }
            }

            image.color = baseColor;
        }

        public int GetManaColor()
        {
            return manaColor;
        }

        public void SetVisualColor(Color color)
        {
            baseColor = color;
            image.color = color;
        }

        public void AnimateMovement(Vector2 from, Vector2 to) {
            image.transform.localPosition = from;
            targetPosition = to;
            speed = initialSpeed;
            moving = true;
        }

        private void Update() {
            if (moving) {
                if (image.transform.localPosition == targetPosition) {
                    moving = false;
                    if (onFallAnimComplete != null) onFallAnimComplete();
                } else {
                    image.transform.localPosition = Vector2.MoveTowards(image.transform.localPosition, targetPosition, speed*Time.smoothDeltaTime);
                    speed += acceleration*Time.smoothDeltaTime;
                }
            }

            // Animate glow
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
            image.color = Color.Lerp(baseColor, litColor, glow);
        }

        /// <summary>
        /// Runs the stored beforeClear method.
        /// Is run before the tiles are damage calculated and removed from the board.
        /// </summary>
        /// <param name="blob">the blob this is in, or null if not in a blob</param>
        public void BeforeClear(GameBoard.Blob blob) {
            if (beforeClear != null) beforeClear(blob);
        }

        public void MakeTrashTile() {
            trashTile = true;
            pointMultiplier -= 1.00f;
            SetVisualColor(Color.Lerp(Color.black, image.color, 0.7f));
        }

        public void MakeObscuresColor() {
            obscuresColor = true;
        }

        public void DontDoGravity() {
            doGravity = false;
        }

        public void MakeFragile() {
            fragile = true;
        }

        public void Obscure() {
            // If this itself is an obscuring tile, do not obscure it
            if (obscuresColor) return;

            if (!obscured) {
                obscured = true;
                image.sprite = obscuredSprite;
                baseColor = obscureColor;
                image.color = obscureColor;
            }
        }

        public void Unobscure(GameBoard board) {
            if (obscured) {
                obscured = false;
                SetManaColor(manaColor, board);
            }
        }

        public void AnimateGlow(float target, float duration) {
            glowStart = glow;
            glowTarget = target;
            glowStartTime = Time.time;
            glowDuration = duration;
        }
    }
}
