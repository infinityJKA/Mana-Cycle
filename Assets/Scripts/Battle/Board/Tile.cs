using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battle.Cycle;

namespace Battle.Board {
    public class Tile : MonoBehaviour
    {
        // Mana color enum value for this tile
        public ManaColor color { get; private set; }
        // Seperate image object attached to this
        public Image image;
        // Target position of this element
        private Vector3 targetPosition;
        // Initial movement speed of this object when movmenet animated - distance in tiles per sec
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
        

        public void SetColor(ManaColor color, GameBoard board, bool ghost = false, bool setColor = true, bool setSprite = true)
        {
            this.color = color;
            // Get image and set color from the list in this scene's cycle
            if (setColor) {
                baseColor = board.cycle.GetManaColor( (int)color );
                litColor = board.cycle.GetLitManaColor( (int)color );
            }
            if (setSprite && board.cycle.usingSprites) {
                if (ghost) {
                    image.sprite = board.cycle.ghostManaSprites[ ((int)color) ];
                    baseColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.4f);
                    image.GetComponent<UnityEngine.UI.Outline>().enabled = true;
                    // image.GetComponent<UnityEngine.UI.Outline>().effectColor = Color.Lerp(image.color, Color.white, 0.4f);
                    image.GetComponent<UnityEngine.UI.Outline>().effectColor = baseColor;
                } else {
                    image.sprite = board.cycle.manaSprites[ ((int)color) ];
                }
            }

            image.color = baseColor;
        }

        public ManaColor GetManaColor()
        {
            return color;
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
                    image.transform.localPosition = Vector2.MoveTowards(image.transform.localPosition, targetPosition, speed*Time.deltaTime);
                    speed += acceleration*Time.deltaTime;
                }
            }

            // Animate glow
            if (Time.time-glowStartTime < glowDuration) {
                glow = Mathf.Lerp(glowStart, glowTarget, (Time.time-glowStartTime)/glowDuration);
                Debug.Log("glow = "+glow);
            }
            // else, pulse glow if glowTarget is 0 (not animateing to a non-zero glow value), & not animating which previous condition stops
            else if (pulseGlow && glowTarget == 0) {
                glow = 0.5f + Mathf.PingPong(Time.time*1.5f, 0.25f);
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

        public void MakeTrashTile(GameBoard board) {
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
                image.color = obscureColor;
            }
        }

        public void Unobscure(GameBoard board) {
            if (obscured) {
                obscured = false;
                SetColor(color, board);
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
