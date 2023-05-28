using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Battle.Board;

namespace Battle {
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


        // Update is called once per frame
        void Update()
        {
            // stop everything while not player controlled, uninitialized, paused, post game or dialogue
            if ( !board.IsPlayerControlled() || !board.isInitialized() || board.convoPaused
                // menu controls above here if ever any
                || board.recoveryMode || board.isPaused() && board.isPostGame() || !board.Mobile ) return;

            // ---- Piece movement ----
            if (Input.touchCount > 0) {
                touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began) {
                    DragStart();
                }

                else if (touch.phase == TouchPhase.Moved) {
                    Dragging();
                }

                else if (touch.phase == TouchPhase.Ended) {
                    DragRelease();
                }
            }

            // Mouse controls - for use in editor
            if (doMouse && Input.GetMouseButtonDown(0)) {
                DragStartMouse();
            }

            else if (doMouse && beingMouseDragged) {
                if (Input.GetMouseButtonUp(0)) {
                    beingMouseDragged = false;
                    DragMouseRelease();
                } else {
                    DraggingMouse();
                }
            }
        }

        void DragStart() {
            Debug.Log(touch);
            dragStartPos = touch.position;
            startCol = board.GetPiece().GetCol();
            deltaCol = 0;
            touchTime = Time.time;
        }

        void DragStartMouse() {
            dragStartPos = Input.mousePosition;
            startCol = board.GetPiece().GetCol();
            deltaCol = 0;
            touchTime = Time.time;
            beingMouseDragged = true;
        }

        void Dragging() {
            int targetDeltaCol = Mathf.RoundToInt(((Vector2)Input.mousePosition - dragStartPos).x * sensitivity * 0.01f);

            while (deltaCol < targetDeltaCol) {
                if (board.MoveRight()) deltaCol++;
                else break;
            }

            while (deltaCol > targetDeltaCol) {
                if (board.MoveLeft()) deltaCol--;
                else break;
            }

            board.quickFall = ((Vector2)Input.mousePosition - dragStartPos).y * sensitivity * 0.01f <= -0.65f;
        }

        void DraggingMouse() {
            int targetDeltaCol = Mathf.RoundToInt(((Vector2)Input.mousePosition - dragStartPos).x * sensitivity * 0.01f);

            while (deltaCol < targetDeltaCol) {
                if (board.MoveRight()) deltaCol++; 
                else break;
            }

            while (deltaCol > targetDeltaCol) {
                if (board.MoveLeft()) deltaCol--;
                else break;
            }

            board.quickFall = ((Vector2)Input.mousePosition - dragStartPos).y * sensitivity * 0.01f <= -0.65f;
        }

        void DragRelease() {
            // only a tap and not a drag if finger moved less than half a tile's width, the minimum to move a tile left/right
            if (Time.time - touchTime < 0.2f && (touch.position - dragStartPos).magnitude < 50f) {
                board.RotateRight();
            }
            // If finger swiped about a tile and a half's distance up quickly, spellcast
            // tile must also have moved less than a tile's width horizontally
            // possible future improvement: make based on angle instead of specific area
            if (Time.time - touchTime < 0.5f && touch.position.y - dragStartPos.y > 150f && Mathf.Abs(touch.position.x - dragStartPos.x) < 100f) {
                board.Spellcast();
            }

            board.quickFall = false;
        }

        void DragMouseRelease() {
            // only a tap and not a drag if mouse moved less than half a tile's width, the minimum to move a tile left/right
            var dragDistance = ((Vector2)Input.mousePosition - dragStartPos).magnitude;
            if (Time.time - touchTime < 0.2f && ((Vector2)Input.mousePosition - dragStartPos).magnitude < 50f) {
                board.RotateRight();
            }
            // If mouse swiped up quickly, spellcast
            Debug.Log(Input.mousePosition.x - dragStartPos.x);
            if (Time.time - touchTime < 0.5f && Input.mousePosition.y - dragStartPos.y > 150f && Mathf.Abs(Input.mousePosition.x - dragStartPos.x) < 100f) {
                board.Spellcast();
            }

            board.quickFall = false;
        }
    }
}
