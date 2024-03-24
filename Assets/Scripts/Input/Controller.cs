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
using VersusMode;

namespace Battle {
    /// <summary>
    /// Allows the player to control a board with keyboard inputs.
    /// </summary>
    public class Controller : MonoBehaviour
    {
        // CharSelectController to delegate inputs to during char select
        private CharSelector charSelector;

        // the board being controlled by this script
        private Board.GameBoard board;

        private ControlMode controlMode;

        public enum ControlMode {
            CharSelector,
            Board
        }

        public PlayerInput playerInput {get; private set;}

        // Movement input for joysticks, etc.
        // This is NOT the same as single button presses such as the arrow keys.
        private Vector2 movementInput;

        // Whether or not to respond to Input Script update-based inputs instead of the new action input system stuff
        [SerializeField] private bool battleUseInputScripts = false;
        [SerializeField] private bool menuUseInputScripts = true;

        [SerializeField] private GameObject moveSFX;

        // set to false whhen this is an AI vs AI match and htis controller can only interact with the pause menu.
        public bool canControlBoard = true;

        private bool gameFocused => !PopupManager.showingPopup;

        private void Awake() {
            playerInput = GetComponent<PlayerInput>();
        }

        // Update is called once per frame
        void Update()
        {

            if (controlMode != ControlMode.Board) return;

            // if not using tinput scripts, this script's update logic is not needed. (OnMove etc will still work when disabled, innvoked by playerinput script on same object.)
            if (!battleUseInputScripts && !menuUseInputScripts) {
                enabled = false;
                return;
            }

            // stop movement while uninitialized, paused, post game or dialogue
            if (!board.IsBattleStarted() || board.convoPaused) return;
            // note that this doesn't check if boaard is player controlled
            // this is so the player can control the menu in AI vs AI mode

            // these will be set to true if any of the input scripts trigger it
            if (battleUseInputScripts && board.IsPlayerControlled()) {
                if (!board.Mobile) board.quickFall = false;
            }

            foreach (InputScript inputScript in board.inputScripts) {
                // if not player controlled AND not player one, skip, only allow player 1 to control menus even if AI
                if (!board.IsPlayerControlled() && board.GetPlayerSide() == 1) continue;
 
                if (inputScript != null)
                {
                    if (Input.GetKeyDown(inputScript.Pause) && menuUseInputScripts)
                    {
                        board.Pause();
                    }
                    
                    Storage.convoEndedThisInput = false;

                    // control the pause menu if paused
                    if (menuUseInputScripts && board.pauseMenu.paused && !board.postGame)
                    {
                        if (Input.GetKeyDown(inputScript.Up)) {
                            board.pauseMenu.MoveCursor(Vector3.up);
                            board.PlaySFX(moveSFX);
                        } else if (Input.GetKeyDown(inputScript.Down)) {
                            board.pauseMenu.MoveCursor(Vector3.down);
                            board.PlaySFX(moveSFX);
                        }

                        if (Input.GetKeyDown(inputScript.Cast)){
                            board.pauseMenu.SelectOption();
                        }           
                    }

                    // same with post game menu, if timer is not running
                    else if (menuUseInputScripts && board.postGame && !board.winMenu.timerRunning)
                    {
                        if (board.winMenu == null) return;
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
                    else if (battleUseInputScripts && !board.isPaused() && !board.isPostGame()) {
                        // code previously in controller.cs
                        if (Input.GetKeyDown(inputScript.RotateCW)){
                            board.RotateCCW();
                        }

                        if (Input.GetKeyDown(inputScript.RotateCCW)){
                            board.RotateCW();
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
                            board.TrySpellcast();
                        }

                        if (board.IsPlayerControlled() && board.GetPiece() != null) {
                            // don't set to false if already true
                            board.quickFall = Input.GetKey(inputScript.Down) || board.quickFall;
                            board.instaDropThisFrame = Input.GetKeyDown(inputScript.Up) || board.instaDropThisFrame;
                        }

                        if (board.Mobile && Input.GetKeyUp(inputScript.Down)) {
                            board.quickFall = false;
                        }
                    }
                }
            }        
        }

        public void SetBoard(GameBoard board) {
            if (!board || !board.gameObject.activeInHierarchy) {
                Debug.LogWarning("Trying to assign to disabled board");
                return;
            }
            this.board = board;
            controlMode = ControlMode.Board;
        }

        public void SetCharSelector(CharSelector charSelector) {
            this.charSelector = charSelector;
            controlMode = ControlMode.CharSelector;
            playerInput = GetComponent<PlayerInput>();
            charSelector.UpdateInputPrompts(playerInput);
        }

        private static float deadzone = 0.1f;
        private static float charSelectInputMagnitude = 0.5f;

        private bool joystickPressed;
        bool joystickPressedSouth = false;
        bool quickfallButtonPressed = false;

        // Functions to handle new Action Input System invocations

        // charselector only
        public void OnMove(InputAction.CallbackContext ctx) {
            if (controlMode != ControlMode.CharSelector || !charSelector) return;
            if (!gameFocused) return;
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;

            movementInput = ctx.ReadValue<Vector2>();

            if (joystickPressed) {
                if (movementInput.magnitude <= deadzone) {
                    joystickPressed = false;
                }
            }            

            else if (!joystickPressed && movementInput.magnitude >= charSelectInputMagnitude) {
                joystickPressed = true;

                float angle = Vector2.SignedAngle(Vector2.up, movementInput);

                if (Mathf.Abs(angle) < 45f) charSelector.OnMoveUp();
                else if (Mathf.Abs(angle - 180f) < 45f) charSelector.OnMoveDown();
                else if (Mathf.Abs(angle - 90f) < 45f) charSelector.OnMoveLeft();
                else if (Mathf.Abs(angle + 90f) < 45f) charSelector.OnMoveRight();
            } 
        }

        // board only
        public void OnPieceMoveAnalog(InputAction.CallbackContext ctx) {
            if (controlMode != ControlMode.Board || !canControlBoard) return;
            if (!gameFocused) return;


            movementInput = ctx.ReadValue<Vector2>();

            if (joystickPressed) {
                if (movementInput.magnitude <= deadzone) {
                    joystickPressed = false;
                    joystickPressedSouth = false;
                    UpdateQuickfall();
                }
            }            

            else if (!joystickPressed && movementInput.magnitude >= charSelectInputMagnitude) {
                joystickPressed = true;

                float angle = Vector2.SignedAngle(Vector2.up, movementInput);

                if (Mathf.Abs(angle) < 45f) {
                    board.UseAbility();
                    board.instaDropThisFrame = true; // for trainbot
                }

                else if (Mathf.Abs(angle - 180f) < 45f) {
                    joystickPressedSouth = true;
                    UpdateQuickfall();
                }

                else if (Mathf.Abs(angle - 90f) < 45f) {
                    board.MoveLeft();
                }
                else if (Mathf.Abs(angle + 90f) < 45f) {
                    board.MoveRight();
                }
            }
        }

        public void OnQuickfall(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;

            if (controlMode == ControlMode.Board && canControlBoard) {
                if (ctx.performed) {
                    quickfallButtonPressed = true;
                }
                if (ctx.canceled) {
                    quickfallButtonPressed = false;
                }
                UpdateQuickfall();
            }
        }

        public void UpdateQuickfall() {
            board.quickFall = quickfallButtonPressed || joystickPressedSouth;
        }

        public void PieceTapLeft(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return; 
            if (controlMode == ControlMode.Board && canControlBoard) board.MoveLeft();
        }

        public void PieceTapRight(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return;
            if (controlMode == ControlMode.Board && canControlBoard) board.MoveRight();
        }

        public void OnRotateLeft(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return;
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;
            if (controlMode == ControlMode.Board && canControlBoard) board.RotateCCW();
            else if (controlMode == ControlMode.CharSelector) charSelector.OnAbilityInfo();
        }

        public void OnRotateRight(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return;
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;
            if (controlMode == ControlMode.Board && canControlBoard) board.RotateCW();
        }

        public void OnSettings(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return;
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;
            if (controlMode == ControlMode.CharSelector) charSelector.OnSettings();
        }

        public void OnSpellcast(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return;
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;
            if (controlMode == ControlMode.Board && canControlBoard) board.TrySpellcast();
            else if (controlMode == ControlMode.CharSelector) charSelector.OnCast(true);
        }

        public void OnAbiltyUse(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return;
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;
            if (controlMode == ControlMode.Board && canControlBoard) {
                board.UseAbility();
                board.instaDropThisFrame = true; // for trainbot
            }
        }

        public void OnCancel(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (!ctx.performed) return;
            if (SidebarUI.instance && SidebarUI.instance.expanded) return;
            else if (controlMode == ControlMode.CharSelector) {
                if (ctx.performed) {
                    charSelector.OnBack();
                }
                else if (ctx.canceled) {
                    charSelector.ReturnMenuUnpress();
                }
            }
        }

        public void OnPause(InputAction.CallbackContext ctx) {
            if (!gameFocused) return;
            if (controlMode == ControlMode.Board && ctx.performed) board.Pause();
        }

        public void EnableInputScripts() {
            Debug.Log("input scripts on "+name+" enabled");
            menuUseInputScripts = true;
            battleUseInputScripts = true;
        }
    }
}
