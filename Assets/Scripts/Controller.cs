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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(inputs);
    }

    // Update is called once per frame
    void Update()
    {
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
}
