using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using Battle.Board;
using Battle.AI;

namespace Battle {
    public class Controller : MonoBehaviour
    {
        // the board being controlled by this script
        [SerializeField] public Board.GameBoard board;

        // Update is called once per frame
        void Update()
        {
            // stop movement while not player controlled, uninitialized, paused, post game or dialogue
            if (!board.IsPlayerControlled() || !board.isInitialized() || board.convoPaused) return;

            // these will be set to true if any of the input scripts trigger it
            if (!board.Mobile) board.quickFall = false;
            board.instaDropThisFrame = false; 

            foreach (InputScript inputScript in board.inputScripts) {
                if (inputScript != null)
                {
                    if (Input.GetKeyDown(inputScript.Pause) && !board.postGame && !Storage.convoEndedThisInput)
                    {
                        board.pauseMenu.TogglePause();
                        board.PlaySFX("pause", pan: 0);
                    }
                    Storage.convoEndedThisInput = false;

                    // control the pause menu if paused
                    if (board.pauseMenu.paused && !board.postGame)
                    {
                        if (Input.GetKeyDown(inputScript.Up)) {
                            board.pauseMenu.MoveCursor(Vector3.up);
                            board.PlaySFX("move", pitch: 0.8f, important: false);
                        } else if (Input.GetKeyDown(inputScript.Down)) {
                            board.pauseMenu.MoveCursor(Vector3.down);
                            board.PlaySFX("move", pitch: 0.75f, important: false);
                        }

                        if (Input.GetKeyDown(inputScript.Cast)){
                            board.pauseMenu.SelectOption();
                        }           
                    }

                    // same with post game menu, if timer is not running
                    else if (board.postGame && !board.winMenu.timerRunning)
                    {
                        if (Input.GetKeyDown(inputScript.Up)) {
                            board.winMenu.MoveCursor(Vector3.up);
                        } else if (Input.GetKeyDown(inputScript.Down)) {
                            board.winMenu.MoveCursor(Vector3.down);
                        }

                        if (Input.GetKeyDown(inputScript.Cast)){
                            board.winMenu.SelectOption();
                        }
                    }

                    // don't evaulate any branches below this if in recovery mode - only menus
                    // also don't control if the player isn't controlling the board
                    else if (board.recoveryMode || !board.IsPlayerControlled()) continue;
                    
                    // If not pausemenu paused, do piece movements if not dialogue paused and not in postgame
                    else if (!board.isPaused() && !board.isPostGame()) {
                        // code previously in controller.cs
                        if (Input.GetKeyDown(inputScript.RotateCW)){
                            board.RotateLeft();
                        }

                        if (Input.GetKeyDown(inputScript.RotateCCW)){
                            board.RotateRight();
                        }

                        if (Input.GetKeyDown(inputScript.Left)){
                            board.MoveLeft();
                        }

                        if (Input.GetKeyDown(inputScript.Right)){
                            board.MoveRight();
                        }

                        if (Input.GetKeyDown(inputScript.Up)){
                            board.UseAbility();
                        }

                        if (Input.GetKeyDown(inputScript.Cast)){
                            board.Spellcast();
                        }

                        if (board.IsPlayerControlled() && board.GetPiece() != null){
                            if (Input.GetKey(inputScript.Down)) board.quickFall = true;
                            if (Input.GetKeyDown(inputScript.Up)) board.instaDropThisFrame = true;
                        }
                    }
                }
            }        
        }
    }
}
