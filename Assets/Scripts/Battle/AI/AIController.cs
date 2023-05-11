// using System;
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
        // Multiplier for how likely the AI is to use their active ability, as opposed to the standard of 1.0.
        // 1.0 is considered the "best" value, above or below should be worse
        public float abilityChanceMultiplier = 1.0f;
        // If true, AI will not wait until piece is in correct rotation to start moving,
        // and will not wait until in correct rotation/column to start quickdropping
        public bool concurrentActions;

        // Movement
        private int move;
        private int targetCol;
        private int targetRot;

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
                board.SetFallTimeMult(1f);
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
                    if (board.GetPiece().GetCol() > this.targetCol){
                        board.MoveLeft();
                    }
                    else if (board.GetPiece().GetCol() < this.targetCol){
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
            // Won't try to spellcast if there are no blobs or already casting
            // Debug.Log("cast chance: "+(CurrentCastChance()*100)+"%");
            if (ShouldCast()){
                board.Spellcast();
            }

            if (ShouldActivateAbility()){
                board.UseAbility();
            }

            // Make different placement decisions based on the effect of the tile being dropped.
            switch (board.GetPiece().effect) {
                // Iron Sword: targets the tallest column always.
                case Battler.ActiveAbilityEffect.IronSword:
                    targetCol = HighestColumn();
                    targetRot = 0;
                    break;

                // Pyro: target a random column.
                // In the future, might make it to where it drops in the position that clears the most tiles
                case Battler.ActiveAbilityEffect.PyroBomb:
                    targetCol = Random.Range(0, 8);
                    targetRot = 0;
                    break;

                // Geo Crystal: targets the lowest column, which is more likely to hit larger mana chunks maybe.
                // probably a smarter approach but that would take coding.
                case Battler.ActiveAbilityEffect.GoldMine:
                    targetCol = LowestColumn();
                    targetRot = 0;
                    break;

                // z?blind: randomly in the center 4 columns
                // probably doesn't need a great AI but maybe target position witht he most unobscured area
                case Battler.ActiveAbilityEffect.ZBlind:
                    targetCol = Random.Range(2, 6);
                    targetRot = 0;
                    break;

                // default: AI best placement algorithm
                default:
                    // Schedule the job that calcualtes the tile position to run later after this frame
                    bestPlacement = new NativeArray<int>(4, Allocator.TempJob);
                    job = new BestPlacementJob(board, bestPlacement, accuracy);
                    placementJobRunning = true;
                    jobHandle = job.Schedule();
                    break;
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
        // Called when a piece is spawned.
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

        // Decides when the AI should activate its ability.
        // Called when a piece is spawned.
        bool ShouldActivateAbility() {
            // don't try to use if not enough mana.
            if (board.abilityManager.mana < board.Battler.activeAbilityMana) return false;

            int rowsFromTop = boardHighestRow - GameBoard.height + GameBoard.physicalHeight;
            switch (board.Battler.activeAbilityEffect) {
                // iron sword: more likely the higher the highest row is
                case Battler.ActiveAbilityEffect.IronSword:
                    // <4 from top: 150% chance, guaranteed unless ability chance multiplier < 0.667
                    if (rowsFromTop < 4 && Random.value < 1.5f*abilityChanceMultiplier) return true;

                    // <8 from top: 70% chance
                    if (rowsFromTop < 8 && Random.value < 0.7f*abilityChanceMultiplier) return true;

                    // <12 from top: 20% chance
                    if (rowsFromTop < 12 && Random.value < 0.2f*abilityChanceMultiplier) return true;

                    // persistent 4% chance
                    if (Random.value < 0.04f*abilityChanceMultiplier) return true;

                    return false;

                // whirlpool: random chance while ready
                case Battler.ActiveAbilityEffect.Whirlpool:
                    // 40% chance while ready
                    return Random.value < 0.35;

                // pyro bomb: same numbers as iron sword, for now
                case Battler.ActiveAbilityEffect.PyroBomb:
                    
                    // <4 from top: 150% chance, guaranteed unless ability chance multiplier < 0.667
                    if (rowsFromTop < 4 && Random.value < 1.5f*abilityChanceMultiplier) return true;

                    // <8 from top: 70% chance
                    if (rowsFromTop < 8 && Random.value < 0.7f*abilityChanceMultiplier) return true;

                    // <12 from top: 20% chance
                    if (rowsFromTop < 12 && Random.value < 0.2f*abilityChanceMultiplier) return true;

                    // persistent 4% chance
                    if (Random.value < 0.04f*abilityChanceMultiplier) return true;

                    return false;

                // foresight: random chance while ready
                case Battler.ActiveAbilityEffect.Foresight:
                    // 40% chance while ready
                    return Random.value < 0.5;

                // gold mine: random chance while ready
                case Battler.ActiveAbilityEffect.GoldMine:
                    // 40% chance while ready
                    return Random.value < 0.25;

                // z?blind: random chance while ready
                case Battler.ActiveAbilityEffect.ZBlind:
                    // 40% chance while ready
                    return Random.value < 0.35;

                default:
                    return false;
            }
        }

        // Returns index of the highest column.
        int HighestColumn() {
            int offset = Random.Range(0, 8);

            int highestRow = 18;
            int highestCol = -1;

            for (int c = offset; c < GameBoard.width+offset; c++) {
                int col = c%8;
                for (int row = GameBoard.height - 1; row >= 0; row--) {
                    if (!board.tiles[row, + col]) {
                        if (row < highestRow) {
                            highestRow = row;
                            highestCol = col;
                        }
                        break;
                    }
                }
            }

            return highestCol;
        }

        // Returns index of the lowest column.
        int LowestColumn() {
            int offset = Random.Range(0, 8);

            int lowestRow = 0;
            int lowestCol = -1;

            for (int c = offset; c < GameBoard.width+offset; c++) {
                int col = c%8;
                for (int row = GameBoard.height - 1; row >= 0; row--) {
                    if (!board.tiles[row, + col]) {
                        if (row > lowestRow) {
                            lowestRow = row;
                            lowestCol = col;
                        }
                        break;
                    }
                }
            }

            return lowestCol;
        }
    }
}