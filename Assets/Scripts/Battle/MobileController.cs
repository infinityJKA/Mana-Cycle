using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Battle.Board;

namespace Battle {
    /// <summary>
    /// Allows the player to control the board using mobile touch-screen inputs.
    /// Also works with mouse for PC debugging, but is disabled in mobile because it conflicts with the touchscreen controls.
    /// </summary>
    public class MobileController : MonoBehaviour
    {
        /// <summary>The board being controlled by this script</summary>
        [SerializeField] public GameBoard board;

        /// <summary>Sensitivity of moving the piece left and right.</summary>
        [SerializeField] float sensitivity;

        /// <summary>If false, mouse inputs will not be registered. Should be set to true when debugging & false when building for phone </summary>
        [SerializeField] private bool doMouse;

        /// <summary>position the payer tapped</summary>
        Vector2 dragStartPos;
        /// <summary>most recent single touch data</summary>
        UnityEngine.Touch touch;
        /// <summary>time that touch occured, used for detecting touch vs. drag</summary>
        private float touchTime;
        /// <summary>amount of cols the piece has currently moved via drag.</summary>
        int deltaCol;

        /// <summary> column of the board's piece when drag began </summary>
        int startCol;

        private bool beingMouseDragged = false;

        void Start() {
            // automatically use a mouse if detected
            // doMouse = Input.mousePresent;
        }

        // Update is called once per frame
        void Update()
        {
            // stop everything while not player controlled, uninitialized, paused, post game or dialogue
            if ( !board.IsPlayerControlled() || !board.IsInitialized() || board.convoPaused
                // menu controls above here if ever any
                || board.recoveryMode || board.IsPaused() && board.IsPostGame() || !board.Mobile ) return;

            // ---- Piece movement ----
            if (Input.touchCount > 0) {
                touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began) {
                    DragStart();
                }

                else if (touch.phase == TouchPhase.Moved) {
                    Drag(touch.position);
                }

                else if (touch.phase == TouchPhase.Ended) {
                    Release(touch.position);
                }
            }

            // Mouse controls - for use in editor
            if (doMouse) {
                if (Input.GetMouseButtonDown(0)) {
                    DragStartMouse();
                }

                else if (beingMouseDragged) {
                    if (Input.GetMouseButtonUp(0)) {
                        beingMouseDragged = false;
                        Release(Input.mousePosition);
                    } else {
                        Drag(Input.mousePosition);
                    }
                }
            }
        }

        void DragStart() {
            dragStartPos = touch.position;
            startCol = board.GetPiece().GetCol();
            deltaCol = 0;
            touchTime = Time.time;
            beingMouseDragged = false;
        }

        void DragStartMouse() {
            dragStartPos = Input.mousePosition;
            startCol = board.GetPiece().GetCol();
            deltaCol = 0;
            touchTime = Time.time;
            beingMouseDragged = true;
        }

        void Drag(Vector2 touchPosition) {
            Vector2 offset = touchPosition - dragStartPos;
            int targetDeltaCol = Mathf.RoundToInt(offset.x * sensitivity * 0.01f);

            while (deltaCol < targetDeltaCol) {
                if (board.MoveRight()) deltaCol++;
                else break;
            }

            while (deltaCol > targetDeltaCol) {
                if (board.MoveLeft()) deltaCol--;
                else break;
            }

            // move about a tile's height down before quick falling (assuming sensitivity of ~0.5)
            board.quickFall = offset.y * sensitivity * 0.01f <= -0.25f;
        }

        void Release(Vector2 touchPosition) {
            Vector2 offset = touchPosition - dragStartPos;

            // only a tap and not a drag if finger moved less than half a tile's width, the minimum to move a tile left/right
            if (Time.time - touchTime < 0.2f && offset.magnitude < 50f) {
                board.RotateCounterClockwise();
            }

            // If slope of y/|x| swipe distance is above a certain threshold, user swiped upwards, spellcast
            // must have also swiped at least one tile's distance upwards (assuming sensitivity = 0.5f)
            var slope = offset.y/Mathf.Abs(offset.x);
            Debug.Log("slope="+slope);
            if (Time.time - touchTime < 0.5f && offset.y*sensitivity > 0.5f && slope > 1.75f) {
                board.Spellcast();
            }

            board.quickFall = false;
            beingMouseDragged = false;
        }
    }
}
