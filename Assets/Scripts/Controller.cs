using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    // the board being controlled by this script
    [SerializeField] private GameBoard board;
    [SerializeField] private InputScript inputs;

    // vars for AI
    private int move;
    private int targetCol;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (board.isPlayerControlled()){
            if (Input.GetKeyDown(inputs.RotateLeft)){
                board.RotateLeft();
            }

            if (Input.GetKeyDown(inputs.RotateRight)){
                board.RotateRight();
            }

            if (Input.GetKeyDown(inputs.Left)){
                board.MoveLeft();
            }

            if (Input.GetKeyDown(inputs.Right)){
                board.MoveRight();
            }

            if (Input.GetKeyDown(inputs.Cast)){
                board.Spellcast();
            }
        }
        else{
            // AI movement
            if (board.isPieceSpawned()){
                // this block runs when a new pieces is spawned
                // TODO make target based on lowest col to survive longer
                targetCol = (int) UnityEngine.Random.Range(0f,(float) GameBoard.width);
            }
            // random number so ai moves at random intervals
            if (((int) UnityEngine.Random.Range(0f,50f) == 0) && !board.isDefeated()){
                
                // random number to choose what to do
                move = (int) UnityEngine.Random.Range(0f, 7f);

                // move the piece to our target col
                if (board.getPiece().GetCol() > this.targetCol){
                    board.MoveLeft();
                }
                else if (board.getPiece().GetCol() < this.targetCol){
                    board.MoveRight();
                }

                switch(move){
                    case 0: board.RotateLeft(); break;
                    case 1: board.RotateRight(); break;
                    case 2: board.Spellcast(); break;
                }

                
            }

        }

    }
}
