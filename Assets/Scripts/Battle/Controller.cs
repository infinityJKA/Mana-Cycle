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
    /// <summary>
    /// Allows the player to control a board with keyboard inputs.
    /// </summary>
    public class Controller : MonoBehaviour
    {
        // the board being controlled by this script
        [SerializeField] public Board.GameBoard board;

        // Update is called once per frame
        void Update()
        {
            // stop movement while uninitialized, paused, post game or dialogue
            if (!board.isInitialized() || board.convoPaused) return;
            // note that this doesn't check if boaard is player controlled
            // this is so the player can control the menu in AI vs AI mode

            // these will be set to true if any of the input scripts trigger it
            if (board.IsPlayerControlled()) {
                if (!board.Mobile) board.quickFall = false;
                board.instaDropThisFrame = false; 
            }

            foreach (InputScript inputScript in board.inputScripts) {
                // if not player controlled AND not player one, skip, only allow player 1 to control menus even if AI
                if (!board.IsPlayerControlled() && board.GetPlayerSide() == 1) continue;
 
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

                    // don't evaulate any branches below this if in recovery mode - only menus above this willl be evaluated
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

                        if (board.IsPlayerControlled() && board.GetPiece() != null) {
                            // don't set to false if already true
                            board.quickFall = Input.GetKey(inputScript.Down) || board.quickFall;
                            board.instaDropThisFrame = Input.GetKeyDown(inputScript.Up);
                        }

                        if (board.Mobile && Input.GetKeyUp(inputScript.Down)) {
                            board.quickFall = false;
                        }
                    }
                }
            }        
        }
    }
}
