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
    private int move;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (board.getControlled()){
            if (Input.GetKeyDown(inputs.RotateLeft)){
                board.rotateLeft();
            }

            if (Input.GetKeyDown(inputs.RotateRight)){
                board.rotateRight();
            }

            if (Input.GetKeyDown(inputs.Left)){
                board.moveLeft();
            }

            if (Input.GetKeyDown(inputs.Right)){
                board.moveRight();
            }

            if (Input.GetKeyDown(inputs.Cast)){
                board.spellcast();
            }
        }
        else{
            // AI movement
            // random number so ai moves at random intervals
            if (((int) UnityEngine.Random.Range(0f,25f) == 0) && !board.getDefeated()){
                
                // random number to choose what to do
                move = (int) UnityEngine.Random.Range(0f, 7f);
                
                // smartest infinity main
                switch(move){
                    case 0: board.moveLeft(); break;
                    case 1: board.moveLeft(); break;
                    case 2: board.moveRight(); break;
                    case 3: board.moveRight(); break;
                    case 4: board.rotateLeft(); break;
                    case 5: board.rotateRight(); break;
                    case 6: board.spellcast(); break;
                }
                
            }

        }

    }
}
