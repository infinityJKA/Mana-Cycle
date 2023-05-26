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

        private bool doMouse = false;

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
                    DragRelease();
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
            int targetDeltaCol = Mathf.RoundToInt(((Vector2)Input.mousePosition - dragStartPos).x * sensitivity * 0.02f);

            while (deltaCol < targetDeltaCol) {
                if (board.MoveRight()) deltaCol++;
                else break;
            }

            while (deltaCol > targetDeltaCol) {
                if (board.MoveLeft()) deltaCol--;
                else break;
            }

            board.quickFall = ((Vector2)Input.mousePosition - dragStartPos).y * sensitivity * 0.02f <= -0.65f;
        }

        void DraggingMouse() {
            int targetDeltaCol = Mathf.RoundToInt(((Vector2)Input.mousePosition - dragStartPos).x * sensitivity * 0.02f);

            while (deltaCol < targetDeltaCol) {
                if (board.MoveRight()) deltaCol++; 
                else break;
            }

            while (deltaCol > targetDeltaCol) {
                if (board.MoveLeft()) deltaCol--;
                else break;
            }

            board.quickFall = ((Vector2)Input.mousePosition - dragStartPos).y * sensitivity * 0.02f <= -0.65f;
        }

        void DragRelease() {
            if (Time.time - touchTime < 0.2f /** && (touch.position - dragStartPos).sqrMagnitude < 10f **/) {
                board.RotateRight();
            }

            board.quickFall = false;
        }
    }
}
