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
        [SerializeField] private InputScript[] inputScripts;
        [SerializeField] private InputScript[] soloInputScripts;

        // Start is called before the first frame update
        void Start()
        {
            // if in solo mode, use solo additional inputs
            if (Storage.gamemode == Storage.GameMode.Solo) inputScripts = soloInputScripts;
        }

        // Update is called once per frame
        void Update()
        {
            // stop movement while not player controlled, uninitialized, paused, post game or dialogue
            if (!board.IsPlayerControlled() || !board.isInitialized() || board.isPaused() || board.isPostGame() || board.convoPaused) return;

            foreach (InputScript inputScript in inputScripts) {
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
            }
        }
    }
}
