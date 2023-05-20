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
        [Tooltip("Delay between movements, lower value is faster.")]
        public float moveDelay = 0.5f;
        // Accuracy - how often the AI chooses "best" mana netting position instead of just lowest column, as a percentage
        [Range(0, 1)]
        [Tooltip("How intelligent the AI\'s placements are.")]
        public float accuracy = 1.0f;
        // Multiplier for how likely the AI is to spellcast, as opposed to the standard of 1.0.
        // 1.0 is considered the "best" value, above or below should be worse
        [Tooltip("Affects how often the AI spellcasts.")]
        public float castChanceMultiplier = 1.0f;
        // Multiplier for how likely the AI is to use their active ability, as opposed to the standard of 1.0.
        // 1.0 is considered the "best" value, above or below should be worse
        [Tooltip("Affects how often the AI will use their ability.")]
        public float abilityChanceMultiplier = 1.0f;
        // If true, AI will not wait until piece is in correct rotation to start moving,
        // and will not wait until in correct rotation/column to start quickdropping
        [Tooltip("AI will rotate, move and drop at the same time.")]
        public bool concurrentActions;

        // Movement
        private int move;
        private int targetCol;
        private int targetRot;
        private int minCol, maxCol;

        private float nextMoveTime;

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

        // rows from top - used in various methods
        int rowsFromTop;

        // when true, insta-drop on next movement tick
        bool readyToInstaDrop = true;
        void Update() {
            // stop while not player controlled, uninitialized, paused, post game or dialogue
            if (board.IsPlayerControlled() || !board.isInitialized() || board.isPaused() || board.isPostGame() || board.convoPaused || board.recoveryMode) return;

            // Will be run the frame a piece is spawned
            if (board.pieceSpawned){
                board.pieceSpawned = false;
                board.quickFall = false;
                minCol = 0;
                maxCol = 7;
                MakePlacementDecision();
            }

            // ai moves at timed intervals
            if ((nextMoveTime - Time.time <= 0) && !board.IsDefeated()){
                // set timer for next move. //// get highest col height so ai speeds up when closer to topout
                // nextMoveTimer = Time.time + Math.Max(UnityEngine.Random.Range(0.5f,0.8f) - (double) board.getColHeight(FindNthLowestCols(GameBoard.width-1)[0])/15, 0.05f);
                nextMoveTime = Time.time + UnityEngine.Random.Range(moveDelay*0.5f, moveDelay*1.5f);

                if (readyToInstaDrop) {
                    readyToInstaDrop = false;
                    board.instaDropThisFrame = true;
                    return;
                }
                
                // rotate piece to target rot
                bool reachedTargetRot = (int)board.GetPiece().getRot() == this.targetRot;
                if (!reachedTargetRot){
                    board.RotateLeft();
                }
                
                // move the piece to our target col, only if rot is met (or if concurrent actions is enabled)
                if (reachedTargetRot || concurrentActions)
                {
                    if (board.GetPiece().GetCol() > this.targetCol){
                        // if piece cannot move, update min/max col and recalculate position
                        if (!board.MoveLeft() && !placementJobRunning) {
                            minCol = board.GetPiece().GetCol();
                            MakePlacementDecision();
                        }
                    }
                    else if (board.GetPiece().GetCol() < this.targetCol){
                        if (!board.MoveRight() && !placementJobRunning) {
                            maxCol = board.GetPiece().GetCol();
                            MakePlacementDecision();
                        }
                    }
                }

                // quick-drop when the target rotation and column are met (or if concurrent actions)
                bool reachedTargetCol = board.GetPiece().GetCol() == this.targetCol;
                if ((reachedTargetRot && reachedTargetCol) || concurrentActions) {
                    // will not insta-drop if highest row is >4 from top
                    if (board.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Instadrop) {

                        // Only insta-drop once the piece is in the correct spot, even if concurrent actions
                        var inPosition = (reachedTargetRot && reachedTargetCol);

                        // stop if any column is very high
                        if (rowsFromTop < 2) {
                            if (!job.willKill && inPosition) board.quickFall = true;
                            return;
                        }

                        // if any of the 4 middle columns are too high, cease insta drop
                        for (int c = 2; c < 6; c++) {
                            if (ColHighestRow(c) < 4) {
                                if (!job.willKill) board.quickFall = true;
                                return;
                            }
                        }

                        if (inPosition) {
                            readyToInstaDrop = true;
                            nextMoveTime = Time.time + UnityEngine.Random.Range(moveDelay*0.3f, moveDelay*0.9f);
                        }
                    } else {
                        if (!job.willKill) board.quickFall = true;
                    }
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

                // default: AI best placement algorithm
                default:
                    // Schedule the job that calcualtes the tile position to run later after this frame
                    bestPlacement = new NativeArray<int>(4, Allocator.TempJob);

                    // when nearing top, prioritize current color
                    var prioritizeColor = rowsFromTop < 6 ? board.GetCycleColor() : Cycle.ManaColor.Any;

                    job = new BestPlacementJob(board, bestPlacement, accuracy, prioritizeColor, minCol, maxCol);
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
                rowsFromTop = boardHighestRow - GameBoard.height + GameBoard.physicalHeight;
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

            // Chance is based on height from top of physical board
            // recovery mode (<4 from top): pretty much always guaranteed, 150% chance, always unless cast chance multiplier < 0.667
            if (rowsFromTop < 4 && Random.value < 1.50f*castChanceMultiplier) return true;

            // <7 from top: 40% chance
            if (rowsFromTop < 7 && Random.value < 0.40f*castChanceMultiplier) return true;

            // <10 from top: 15% chance
            if (rowsFromTop < 10 && Random.value < 0.15f*castChanceMultiplier) return true;

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

            switch (board.Battler.activeAbilityEffect) {
                // iron sword: more likely the higher the highest row is
                case Battler.ActiveAbilityEffect.IronSword:
                    // if the highest column is less than 3 tiles high, do not use
                    if (rowsFromTop > 11) return false;

                    // <4 from top: 200% chance, guaranteed unless ability chance multiplier < 0.5
                    if (rowsFromTop < 4 && Random.value < 2.00f*abilityChanceMultiplier) return true;

                    // <7 from top: 75% chance
                    if (rowsFromTop < 7 && Random.value < 0.75f*abilityChanceMultiplier) return true;

                    // <10 from top: 25% chance
                    if (rowsFromTop < 10 && Random.value < 0.25f*abilityChanceMultiplier) return true;

                    // persistent 4% chance
                    if (Random.value < 0.04f*abilityChanceMultiplier) return true;

                    return false;

                // whirlpool: random chance while ready
                case Battler.ActiveAbilityEffect.Whirlpool:
                    return Random.value < 0.35*abilityChanceMultiplier;

                // pyro bomb: same numbers as iron sword, for now
                case Battler.ActiveAbilityEffect.PyroBomb:
                    // if the highest column is less than 3 tiles high, do not use
                    if (rowsFromTop > 11) return false;

                    // <4 from top: 200% chance, guaranteed unless ability chance multiplier < 0.5
                    if (rowsFromTop < 4 && Random.value < 2.00f*abilityChanceMultiplier) return true;

                    // <7 from top: 75% chance
                    if (rowsFromTop < 7 && Random.value < 0.75f*abilityChanceMultiplier) return true;

                    // <10 from top: 25% chance
                    if (rowsFromTop < 10 && Random.value < 0.25f*abilityChanceMultiplier) return true;

                    // persistent 4% chance
                    if (Random.value < 0.04f*abilityChanceMultiplier) return true;

                    return false;

                // foresight: random chance while ready
                case Battler.ActiveAbilityEffect.Foresight:
                    return Random.value < 0.5*abilityChanceMultiplier;

                // gold mine: random chance while ready
                case Battler.ActiveAbilityEffect.GoldMine:
                    return Random.value < 0.35*abilityChanceMultiplier;

                // z?blind: random chance while ready
                case Battler.ActiveAbilityEffect.ZBlind:
                    return Random.value < 0.35*abilityChanceMultiplier;

                // default: random chance
                default:
                    return Random.value < 0.5*abilityChanceMultiplier;
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

        int ColHighestRow(int col) {
            for (int row = GameBoard.height - 1; row >= 0; row--) {
                if (!board.tiles[row, col]) {
                    return row;
                }
            }
            return 0;
        }
    }
}