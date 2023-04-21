using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battle.Board;

namespace Battle {
    public class Controller : MonoBehaviour
    {
        // the board being controlled by this script
        [SerializeField] private Board.GameBoard board;
        [SerializeField] private InputScript[] inputScripts;
        [SerializeField] private InputScript[] soloInputScripts;

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
        private double nextMoveTimer = 0d;

        // Start is called before the first frame update
        void Start()
        {
            // if in solo mode, use solo additional inputs
            if (Storage.gamemode == Storage.GameMode.Solo) inputScripts = soloInputScripts;
            
        }

        // Update is called once per frame
        void Update()
        {
            if (!board.isInitialized()) return;

            // stop movement while paused, post game or dialogue
            if (board.isPaused() || board.isPostGame() || board.convoPaused) return;

            if (board.isPlayerControlled() && !board.isDefeated()){
                foreach (InputScript inputScript in inputScripts) {
                    if (Input.GetKeyDown(inputScript.RotateRight)){
                        board.RotateLeft();
                    }

                    if (Input.GetKeyDown(inputScript.RotateLeft)){
                        // Debug.Log( "rot:" + ((int) board.getPiece().getRot()) +  "col:" + ((int) board.getPiece().GetCol()));
                        board.RotateRight();
                    }

                    if (Input.GetKeyDown(inputScript.Left)){
                        // Debug.Log( "rot:" + ((int) board.getPiece().getRot()) +  "col:" + ((int) board.getPiece().GetCol()));
                        board.MoveLeft();
                    }

                    if (Input.GetKeyDown(inputScript.Right)){
                        // Debug.Log( "rot:" + ((int) board.getPiece().getRot()) +  "col:" + ((int) board.getPiece().GetCol()));
                        board.MoveRight();
                    }

                    if (Input.GetKeyDown(inputScript.Cast)){
                        board.Spellcast();
                    }
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
                    if (board.getColHeight(FindLowestCols()[0]) > GameBoard.height/2){
                        move = 0;
                    }
                    else{
                        move = (int) UnityEngine.Random.Range(0f, 7f);
                    }
                
                    // don't try to spellcast if there are no blobs
                    if (move == 0 && board.GetBlobCount()>0){
                        board.Spellcast();
                    }
                }
                // ai moves at timed intervals
                if ((nextMoveTimer - Time.time <= 0) && !board.isDefeated()){

                    // set timer for next move. get col height so ai speeds up when closer to topout
                    nextMoveTimer = Time.time + Math.Max(UnityEngine.Random.Range(0.6f,1f) - (double) board.getColHeight(FindLowestCols()[0])/20, 0.2f);
                
                    
                    // rotate peice to target rot
                    if ((int) board.getPiece().getRot() > this.targetRot){
                        // Debug.Log(board.getPiece().getRot());
                    }
                    else if ((int) board.getPiece().getRot() != this.targetRot){
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

                    }
                }

                if (targetCol == board.getPiece().GetCol() && targetRot == (int) board.getPiece().getRot())
                {
                    // we are at target, so quickdrop
                    board.setFallTimeMult(0.1f);
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
}
