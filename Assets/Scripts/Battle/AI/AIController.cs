using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;

using Battle.Board;

namespace Battle.AI {
    public class AIController : MonoBehaviour {
        [SerializeField] public Board.GameBoard board;

        // AI difficulty params
        // Move delay - approximate delay between piece movements, lower is faster
        public float moveDelay = 0.5f;
        // Accuracy - how often the AI chooses "best" mana netting position instead of just lowest column, as a percentage
        [Range(0, 1)]
        public float accuracy = 1.0f;
        // Multiplier for how likely the AI is to spellcast, as opposed to the standard of 1.0.
        // 1.0 is considered the "best" value, above or below should be worse
        public float castChanceMultiplier = 1.0f;
        // If true, AI will not wait until piece is in correct rotation to start moving,
        // and will not wait until in correct rotation/column to start quickdropping
        public bool concurrentActions;

        // Movement
        private int move;
        private int targetCol;
        private int targetRot;
        private int colAdjust;

        private float nextMoveTimer;

        // Column detection (unused)
        // private Tile[,] boardLayout;
        // private int[] heights;
        // private int lowestHeight;
        // private int[] orderedHeights;
        // private List<int> lowestCols = new List<int>();

        // Placement calculation
        // currently running best placement job
        BestPlacementJob job;
        // Schedule handle for the best
        JobHandle jobHandle;
        // Whether or not the job is running
        bool placementJobRunning;

        // Stores the best placement results
        // order: [col, rotation, manaGain, highestTileRow]
        // will be ordered in priority of max manaGain and then max highestTileRow 
        // (higher row number means lower on the board)
        NativeArray<int> bestPlacement;
        
        // Highest row found during the last placement calculation
        int boardHighestRow = GameBoard.height;

        void Update() {
            // stop while not player controlled, uninitialized, paused, post game or dialogue
            if (board.IsPlayerControlled() || !board.isInitialized() || board.isPaused() || board.isPostGame() || board.convoPaused) return;

            // Will be run the frame a piece is spawned
            if (board.pieceSpawned){
                board.pieceSpawned = false;
                MakePlacementDecision();
            }

            // ai moves at timed intervals
            if ((nextMoveTimer - Time.time <= 0) && !board.IsDefeated()){
                // set timer for next move. //// get highest col height so ai speeds up when closer to topout
                // nextMoveTimer = Time.time + Math.Max(UnityEngine.Random.Range(0.5f,0.8f) - (double) board.getColHeight(FindNthLowestCols(GameBoard.width-1)[0])/15, 0.05f);
                nextMoveTimer = Time.time + UnityEngine.Random.Range(moveDelay*0.5f, moveDelay*1.5f);
                
                // rotate piece to target rot
                bool reachedTargetRot = (int)board.GetPiece().getRot() == this.targetRot;
                if (!reachedTargetRot){
                    board.RotateLeft();
                }
                
                // move the piece to our target col, only if rot is met (or if concurrent actions is enabled)
                if (reachedTargetRot || concurrentActions)
                {
                    if (board.GetPiece().GetCol() + colAdjust > this.targetCol){
                        board.MoveLeft();
                    }
                    else if (board.GetPiece().GetCol() + colAdjust < this.targetCol){
                        board.MoveRight();
                    }
                }

                // quick-drop when the target rotation and column are met (or if concurrent actions)
                bool reachedTargetCol = board.GetPiece().GetCol() == this.targetCol;
                if ((reachedTargetRot && reachedTargetCol) || concurrentActions) {
                    board.SetFallTimeMult(0.1f);
                }
            }     
        }


        public void MakePlacementDecision() {
            // ( OLD AI )
            // // TODO factor in making blobs in some way. likely by looping through each possible column and checking blob size / dmg
            // targetCol = FindNthLowestCols(0)[ (int) (UnityEngine.Random.Range(0f, FindNthLowestCols(0).Count)) ];

            // if (targetCol == 7){
            //     // piece can only reach edges in specific rotations.
            //     targetRot = 2;
            // }
            // else if (targetCol == 0){
            //     targetRot = 3;
            // }
            // else{
            //     targetRot = (int) UnityEngine.Random.Range(0f, 4f);
            // }

            // Reset fall mult after old piece was just placed
            board.SetFallTimeMult(1f);

            // Won't try to spellcast if there are no blobs or already casting
            // Debug.Log("cast chance: "+(CurrentCastChance()*100)+"%");
            if (ShouldCast()){
                board.Spellcast();
            }

            // ( NEW AI )
            if (!placementJobRunning) {
                // Schedule the job that calcualtes the tile position to run later after this frame
                bestPlacement = new NativeArray<int>(4, Allocator.TempJob);
                job = new BestPlacementJob(board, bestPlacement, accuracy);
                placementJobRunning = true;
                jobHandle = job.Schedule();
            }
        }

        // Sometime later in the frame, read the best position that the job has calculated during the frame
        private void LateUpdate()
        {
            if (placementJobRunning) {
                jobHandle.Complete(); 

                placementJobRunning = false;
                targetCol = bestPlacement[0];
                targetRot = bestPlacement[1];
                // Debug.Log("col="+targetCol+", rot="+targetRot+" - manaGain="+bestPlacement[2]+", highestRow="+bestPlacement[3]);
                boardHighestRow = job.boardHighestRow;
                bestPlacement.Dispose();
                job.boardTiles.Dispose();
                job.accuracyRng.Dispose();
            }
        }

        // Decides if this AI should spellcast now or not.
        // Called when a tile is spawned.
        bool ShouldCast() {
            // board.getColHeight(FindNthLowestCols(0)[0]) > GameBoard.height/2 && board.GetBlobCount()>0 && !board.GetCasting()
            
            // do not cast at all if there are no blobs, or if already casting
            if (board.GetBlobCount() <= 0 || board.GetCasting()) return false;

            // Note: All these chances stack on top of each other, affecting overall cast chance

            int incomingDamage = board.hpBar.TotalIncomingDamage();
            // Incoming damage will kill: 100% chance, guaranteed unless cast chance multiplier < 1.0
            if (incomingDamage >= board.hp && Random.value < 1.0f*castChanceMultiplier) return true;

            // Incoming damage greater than 600: 40% chance
            if (incomingDamage >= 600 && Random.value < 0.4f*castChanceMultiplier) return true;

            // Incoming damage greater than 0: 8% chance
            if (incomingDamage > 0 && Random.value < 0.08f*castChanceMultiplier) return true;

            int rowsFromTop = boardHighestRow - GameBoard.height + GameBoard.physicalHeight;
            // Chance is based on height from top of physical board
            // <4 from top: 125% chance, guaranteed unless cast chance multiplier < 0.8
            if (rowsFromTop < 4 && Random.value < 1.25f*castChanceMultiplier) return true;

            // <8 from top: 35% chance
            if (rowsFromTop < 8 && Random.value < 0.35f*castChanceMultiplier) return true;

            // <12 from top: 10% chance
            if (rowsFromTop < 12 && Random.value < 0.1f*castChanceMultiplier) return true;

            // Persistent 4% chance
            if (Random.value < 0.04f*castChanceMultiplier) return true;


            // if no random checks landed, don't cast
            return false;
        }

        // Gets the current spellcast chance. Only really for debug
        // float CurrentCastChance() {
        //     // do not cast at all if there are no blobs, or if already casting
        //     // if (board.GetBlobCount() <= 0 || board.GetCasting()) return 0;
        //     // ( Ignores first check, dont care about if the cast will work or not )

        //     float chance = 0f;

        //     void FactorChance(float newChance) {
        //         chance += (1-chance) * newChance * castChanceMultiplier;
        //     }

        //     int incomingDamage = board.hpBar.TotalIncomingDamage();
        //     // Incoming damage will kill: 100% chance, guaranteed unless cast chance multiplier < 1.0
        //     if (incomingDamage >= board.hp) FactorChance(1f);

        //     // Incoming damage greater than 600: 40% chance
        //     if (incomingDamage >= 600) FactorChance(0.4f);

        //     // Incoming damage greater than 0: 8% chance
        //     if (incomingDamage > 0) FactorChance(0.08f);

        //     int rowsFromTop = boardHighestRow - GameBoard.height + GameBoard.physicalHeight;

        //     // Chance is based on height from top of physical board
        //     // <4 from top: 125% chance, guaranteed unless cast chance multiplier < 0.8
        //     if (rowsFromTop < 4) FactorChance(1.25f);

        //     // <8 from top: 35% chance
        //     if (rowsFromTop < 8) FactorChance(0.35f);

        //     // <12 from top: 10% chance
        //     if (rowsFromTop < 12) FactorChance(0.1f);

        //     // Persistent 4% chance
        //     FactorChance(0.04f);


        //     // if no random checks landed, don't cast
        //     return chance;
        // }

        /// <summary>
        /// returns a list of the Nth lowest column numbers, where n=0 returns 1st lowest, n=1 returns the 2nd lowest, so on
        /// </summary>
        // public List<int> FindNthLowestCols(int n){
        //     // slightly awkward naming convention 
        //     boardLayout = board.getBoard();
        //     heights = new int[GameBoard.width];

        //     // loop over cols and create a list with their heights
        //     for (int c = 0; c < boardLayout.GetLength(1); c++){  
        //         heights[c] = board.getColHeight(c);
        //     }
        //     // Debug.Log(heights.ToString());

        //     // we now have a list of all col's heights, left to right.
        //     // now add the column numbers of all the lowest columns in a list.
        //     // first, get the nth lowest height. 
        //     orderedHeights = new int[heights.Length];
        //     Array.Copy(heights, 0, orderedHeights, 0, heights.Length);
        //     Array.Sort(orderedHeights);
        //     lowestHeight = orderedHeights[n];
        //     lowestCols = new List<int>();
        //     for (int i = 0; i < heights.Length; i++){
        //         if (heights[i] == lowestHeight){
        //             lowestCols.Add(i);
        //         }
        //     }

        //     return (lowestCols);
        // }
    }
}