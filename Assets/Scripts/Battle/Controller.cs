using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using Battle.Board;

namespace Battle {
    public class Controller : MonoBehaviour
    {
        // the board being controlled by this script
        [SerializeField] public Board.GameBoard board;
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

        // vars for new AI
        // currently running best placement job
        // BestPlacementJob job;
        // Schedule handle for the best
        JobHandle jobHandle;
        // Stores the best placement results
        public NativeArray<int> bestPlacement;

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

            if (board.IsPlayerControlled() && !board.IsDefeated()){
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
            else{
                // AI movement
                if (board.IsPieceSpawned()){
                    // this block runs when a new pieces is spawned
                    // find cols with the least height and randomly choose between them
                    // // TODO factor in making blobs in some way. likely by looping through each possible column and checking blob size / dmg
                    targetCol = FindNthLowestCols(0)[ (int) (UnityEngine.Random.Range(0f, FindNthLowestCols(0).Count)) ];

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

                    // new AI: will read from the current job's variables
                    
                    board.SetFallTimeMult(1f);

                    // random number to choose when to cast
                    if (board.getColHeight(FindNthLowestCols(0)[0]) > GameBoard.height/2){
                        move = 0;
                    }
                    else{
                        move = (int) UnityEngine.Random.Range(0f, 7f);
                    }
                
                    // don't try to spellcast if there are no blobs or already casting
                    if (move == 0 && board.GetBlobCount()>0 && !board.GetCasting()){
                        board.Spellcast();
                    }

                    // Schedule all 32 placements to be tested
                    // 8 is the batch count; amount of calculations per frame
                    // so if my understanding is right, this should be done in 4 frames
                    // bestPlacement = new NativeArray<int>(3, Allocator.TempJob);
                    // job = new BestPlacementJob(board, bestPlacement);
                    // jobHandle = job.Schedule(32, 1);
                }

                // ai moves at timed intervals
                if ((nextMoveTimer - Time.time <= 0) && !board.IsDefeated()){

                    // set timer for next move. get highest col height so ai speeds up when closer to topout
                    nextMoveTimer = Time.time + Math.Max(UnityEngine.Random.Range(0.5f,0.8f) - (double) board.getColHeight(FindNthLowestCols(GameBoard.width-1)[0])/15, 0.05f);
                    // if (board.GetPlayerSide() == 0) Debug.Log(nextMoveTimer - Time.time);
                
                    
                    // rotate peice to target rot
                    if ((int) board.GetPiece().getRot() > this.targetRot){
                        // Debug.Log(board.getPiece().getRot());
                    }
                    else if ((int) board.GetPiece().getRot() != this.targetRot){
                        board.RotateLeft();
                    }
                    else{

                        // move the piece to our target col, only if rot is met
                        if (board.GetPiece().GetCol() + colAdjust > this.targetCol){
                            board.MoveLeft();
                        }
                        else if (board.GetPiece().GetCol() + colAdjust < this.targetCol){
                            board.MoveRight();
                        }

                    }
                }

                if (targetCol == board.GetPiece().GetCol() && targetRot == (int) board.GetPiece().getRot())
                {
                    // we are at target, so quickdrop
                    board.SetFallTimeMult(0.1f);
                }
            }

        } // close Update()

        // Sometime later in the frame, read the best position that the job has generated
        // private void LateUpdate()
        // {
        //     jobHandle.Complete(); 

        //     targetCol = bestPlacement[0];
        //     targetRot = bestPlacement[1];
        //     bestPlacement.Dispose();
        //     job.boardTiles.Dispose();
        //     // job.virtualTiles.Dispose();

        //     Debug.Log("Saved best placement: "+targetCol+", "+targetRot);
        // }

        /// <summary>
        /// returns a list of the Nth lowest column numbers, where n=0 returns 1st lowest, n=1 returns the 2nd lowest, so on
        /// </summary>
        public List<int> FindNthLowestCols(int n){
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
            // first, get the nth lowest height. 
            orderedHeights = new int[heights.Length];
            Array.Copy(heights, 0, orderedHeights, 0, heights.Length);
            Array.Sort(orderedHeights);
            lowestHeight = orderedHeights[n];
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
