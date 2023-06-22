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
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Battle {
    /// <summary>
    /// Allows the player to control a board with keyboard inputs.
    /// </summary>
    public class Controller : MonoBehaviour /**, IMoveHandler **/
    {
        // the board being controlled by this script
        [SerializeField] public GameBoard board;

        [SerializeField] public InputActionAsset soloActions;

        private PlayerInput playerInput;

        void Start()
        {
            if (!board.IsPlayerControlled()) return;

            if (board.IsSinglePlayer())
            {
                playerInput.actions = soloActions;
                playerInput.defaultControlScheme = "Keyboard&Mouse";
            }

            playerInput.actions.FindActionMap("Battle").Enable();

            // If no input devices are available, use the keyboard - even if the other player inputuser is also paired to it
            if (playerInput.devices.Count == 0)
            {
                Debug.Log("no devices? :(");
                playerInput.SwitchCurrentControlScheme(playerInput.defaultControlScheme, Keyboard.current);
            }

            playerInput.actions["MoveLeft"].performed += board.MoveLeft;

            playerInput.actions["MoveRight"].performed += board.MoveRight;

            playerInput.actions["Quickdrop"].performed += board.StartQuickdrop;

            playerInput.actions["Quickdrop"].performed += board.StartQuickdrop;
            playerInput.actions["Quickdrop"].canceled += board.EndQuickdrop;

            playerInput.actions["Spellcast"].performed += board.Spellcast;

            playerInput.actions["UseAbility"].performed += board.UseAbility;

            playerInput.actions["RotateCounterClockwise"].performed += board.RotateCounterClockwise;

            playerInput.actions["RotateClockwise"].performed += board.RotateClockwise;

            playerInput.actions["Pause"].performed += board.TogglePause;
        }

        // Update is called once per frame
        void Update()
        {
            // stop movement while uninitialized, paused, post game or dialogue
            if (!board.IsInitialized() || board.convoPaused) return;
            // note that this doesn't check if boaard is player controlled
            // this is so the player can control the menu in AI vs AI mode

            // these will be set to true if any of the input scripts trigger it
            if (board.IsPlayerControlled()) {
                // if (!board.Mobile) board.quickFall = false;
                board.instaDropThisFrame = false; 
            }

            // if not player controlled AND not player one, skip, only allow player 1 to control menus even if AI
            if (!board.IsPlayerControlled() && board.GetPlayerSide() == 1) return;

            Storage.convoEndedThisInput = false;


            // control the pause menu if paused
            if (board.pauseMenu.paused && !board.postGame)
            {
                //if (Input.GetKeyDown(inputScript.Up))
                //{
                //    board.pauseMenu.MoveCursor(Vector3.up);
                //    board.PlaySFX("move", pitch: 0.8f, important: false);
                //}
                //else if (Input.GetKeyDown(inputScript.Down))
                //{
                //    board.pauseMenu.MoveCursor(Vector3.down);
                //    board.PlaySFX("move", pitch: 0.75f, important: false);
                //}

                //if (Input.GetKeyDown(inputScript.Cast))
                //{
                //    board.pauseMenu.SelectOption();
                //}
            }

            // same with post game menu, if timer is not running
            else if (board.postGame && !board.winMenu.timerRunning)
            {
                //if (Input.GetKeyDown(inputScript.Up))
                //{
                //    board.winMenu.MoveCursor(Vector3.up);
                //}
                //else if (Input.GetKeyDown(inputScript.Down))
                //{
                //    board.winMenu.MoveCursor(Vector3.down);
                //}

                //if (Input.GetKeyDown(inputScript.Cast))
                //{
                //    board.winMenu.SelectOption();
                //}
            }

            // don't evaulate any branches below this if in recovery mode - only menus above this willl be evaluated
            else if (board.recoveryMode || !board.IsPlayerControlled()) return;

            // If not pausemenu paused, do piece movements if not dialogue paused and not in postgame
            else if (!board.IsPaused() && !board.IsPostGame())
            {
                //// code previously in controller.cs
                //if (Input.GetKeyDown(inputScript.RotateCW))
                //{
                //    board.RotateClockwise();
                //}

                //if (Input.GetKeyDown(inputScript.RotateCCW))
                //{
                //    board.RotateCounterClockwise();
                //}

                //if (Input.GetKeyDown(inputScript.Left))
                //{
                //    board.MoveLeft();
                //}

                //if (Input.GetKeyDown(inputScript.Right))
                //{
                //    board.MoveRight();
                //}

                //if (Input.GetKeyDown(inputScript.Up))
                //{
                //    board.UseAbility();
                //}

                //if (Input.GetKeyDown(inputScript.Cast))
                //{
                //    board.Spellcast();
                //}

                //if (board.IsPlayerControlled() && board.GetPiece() != null)
                //{
                //    // don't set to false if already true
                //    board.quickFall = Input.GetKey(inputScript.Down) || board.quickFall;
                //    board.instaDropThisFrame = Input.GetKeyDown(inputScript.Up);
                //}

                //if (board.Mobile && Input.GetKeyUp(inputScript.Down))
                //{
                //    board.quickFall = false;
                //}
            }
        }

        private void OnValidate()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        //public void OnMove(AxisEventData eventData)
        //{
        //    switch(eventData.moveDir)
        //    {
        //        case MoveDirection.Left:
        //            board.MoveLeft();
        //            break;
        //        case MoveDirection.Right:
        //            board.MoveRight();
        //            break;
        //    }
        //}
    }
}
