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
    private int targetRot;
    private int colAdjust;
    private int[] cLengths;
    private Tile[,] boardLayout;
    private int[] heights;
    private int lowestHeight;
    private int[] orderedHeights;
    private List<int> lowestCols = new List<int>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!board.isInitialized()) return;

        // stop movement while paused or post game
        if (board.isPaused() || board.isPostGame()) return;

        if (board.isPlayerControlled()){
            if (Input.GetKeyDown(inputs.RotateLeft)){
                board.RotateLeft();
            }

            if (Input.GetKeyDown(inputs.RotateRight)){
                // Debug.Log( "rot:" + ((int) board.getPiece().getRot()) +  "col:" + ((int) board.getPiece().GetCol()));
                board.RotateRight();
            }

            if (Input.GetKeyDown(inputs.Left)){
                // Debug.Log( "rot:" + ((int) board.getPiece().getRot()) +  "col:" + ((int) board.getPiece().GetCol()));
                board.MoveLeft();
            }

            if (Input.GetKeyDown(inputs.Right)){
                // Debug.Log( "rot:" + ((int) board.getPiece().getRot()) +  "col:" + ((int) board.getPiece().GetCol()));
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
                // find cols with the least height and randomly choose between them
                // TODO factor in making blobs in some way. likely by looping through each possible column and checking blob size / dmg
                targetCol = FindLowestCols()[ (int) (UnityEngine.Random.Range(0f, FindLowestCols().Count)) ];

                if (targetCol == 7){
                    // piece can only reach edges in specific rotations.
                    targetRot = 2;
                }
                else if (targetCol == 0){
                    targetRot = 3;
                }
                else{
                    targetRot = (int) UnityEngine.Random.Range(0f, 4f);
                }
                
                board.setFallTimeMult(1f);

                // random number to choose when to cast
                if (FindLowestCols()[0] < GameBoard.height/2){
                    move = 0;
                }
                else{
                    move = (int) UnityEngine.Random.Range(0f, 4f);
                }
               

                if (move == 0){
                    board.Spellcast();
                }
            }
            // random number so ai moves at random intervals
            if (((int) UnityEngine.Random.Range(0f,110f) == 0) && !board.isDefeated()){
                
                // rotate peice to target rot
                if ((int) board.getPiece().getRot() > this.targetRot){
                    Debug.Log(board.getPiece().getRot());
                }
                else if ((int) board.getPiece().getRot() < this.targetRot){
                    board.RotateLeft();
                }
                else{

                    // move the piece to our target col, only if rot is met
                    if (board.getPiece().GetCol() + colAdjust > this.targetCol){
                        board.MoveLeft();
                    }
                    else if (board.getPiece().GetCol() + colAdjust < this.targetCol){
                        board.MoveRight();
                    }
                    else{
                        // we are at target, so quickdrop
                        board.setFallTimeMult(0.1f);
                    }
                }
            }
        }

    } // close Update()

    public List<int> FindLowestCols(){
        // slightly awkward naming convention 
        boardLayout = board.getBoard();
        heights = new int[GameBoard.width];

        // loop over cols and create a list with their heights
        for (int c = 0; c < boardLayout.GetLength(1); c++){  
            heights[c] = board.getColHeight(c);
        }
        // Debug.Log(heights.ToString());

        // we now have a list of all col's heights, left to right.
        // now add the column numbers of all the lowest columns in a list.
        // first, get the lowest height. 
        orderedHeights = new int[heights.Length];
        Array.Copy(heights, 0, orderedHeights, 0, heights.Length);
        Array.Sort(orderedHeights); // these in place methods are killing me
        lowestHeight = orderedHeights[0];
        lowestCols = new List<int>();
        for (int i = 0; i < heights.Length; i++){
            if (heights[i] == lowestHeight){
                lowestCols.Add(i);
            }
        }

        return (lowestCols);
    }

}
