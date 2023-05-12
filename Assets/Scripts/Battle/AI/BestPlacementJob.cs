using System;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Battle.Cycle;
using System.Collections.Generic;

using Battle.Board;

namespace Battle.AI {
    public struct BestPlacementJob : IJob
    {
        // ---- Inputs
        // This is a flattened 2D array so that it fits the native array.
        // Coordinate system works like this: array[row * width + col]
        public NativeArray<ManaColor> boardTiles;

        // A percentage of how "accurate" the determined placement should be.
        public float accuracy;
        
        // public NativeArray<VirtualTile> virtualTiles;

        // inner
        int rngConstant;

        // list of bools, used to determine which possible placements the AI will "see" and act upon
        public NativeArray<bool> accuracyRng;

        ManaColor center, top, right;
        int tileIndex;

        // USED IN HIGHEST/LOWEST COLUMN CALCULATIONS oops caps lock
        public (int, int) highLowCols, highLowColHeights;

        // stores if the current placement is invalid
        bool invalidPlacement;

        // ---- Outputs
        // Has 3 variables for the best placement position found.
        // [bestCol, bestRot, bestManaGain, highestRow]
        public NativeArray<int> bestPlacement;

        // Total amount of net mana the current placement has accumulated
        public int manaGain;

        // The highest row (lowest value) on the board that was found during the latest calculation
        public int boardHighestRow;

        public BestPlacementJob(GameBoard board, NativeArray<int> bestPlacement, float accuracy) {
            this.bestPlacement = bestPlacement;
            this.accuracy = accuracy;

            // copy down the state of the board's tiles' colors
            boardTiles = new NativeArray<ManaColor>(GameBoard.height*GameBoard.width, Allocator.Persistent);
            boardHighestRow = 18;
            for (int r = 0; r < GameBoard.height; r++) {
                for (int c = 0; c < GameBoard.width; c++) {
                    if (board.tiles[r, c]) {
                        // If the tile is obscured by zman, AI will be more likely to remember the color based on accuracy
                        // If not, it knows it's a color, but not what color
                        // Even the highest accuracy AI will only remember 70% of the colors at a time, 
                        // just so zman isnt completely useless against high level AI
                        if (board.tiles[r, c].obscured && UnityEngine.Random.value > accuracy*0.7f) {
                            boardTiles[r*GameBoard.width + c] = ManaColor.Colorless;
                        } else {
                            boardTiles[r*GameBoard.width + c] = board.tiles[r, c].color;
                        }

                        boardHighestRow = Math.Min(boardHighestRow, r);
                    } else {
                        boardTiles[r*GameBoard.width + c] = ManaColor.None;
                    }
                    boardTiles[r*GameBoard.width + c] = board.tiles[r, c] ? board.tiles[r, c].color : ManaColor.None;
                }
            }

            var piece = board.GetPiece();
            center = piece.GetCenter().color;
            top = piece.GetTop().color;
            right = piece.GetRight().color;
            tileIndex = 0;
            invalidPlacement = false;

            manaGain = 0; // net mana gain for the current placement

            // randomly offset the starting point
            // purpose is to mox in some of the other best placements from the other side of the board
            rngConstant = UnityEngine.Random.Range(0, 32);

            // set accuracy rng - each index represents a possible placement that the AI will or will not see
            accuracyRng = new NativeArray<bool>(32, Allocator.TempJob);
            for (int i=0; i<32; i++) {
                accuracyRng[i] = (UnityEngine.Random.value < accuracy);
            }

            // first item is highest column (lowest row value)
            // second item is lowest column (highest row value)
            highLowCols = (-1, -1);
            highLowColHeights = (18, 0);

            // Find the column numbers for the hgihest and lowest columns.
            // Start with a random offset, to include more randomness
            int offset = rngConstant%8;
            for (int c = offset; c < GameBoard.width+offset; c++) {
                int col = c%8;
                var height = 0;
                for (int row = GameBoard.height - 1; row >= 0; row--) {
                    if (boardTiles[row*GameBoard.width + col] == ManaColor.None) {
                        height = row;
                        break;
                    }
                }

                if (height < highLowColHeights.Item1) {
                    highLowCols.Item1 = col;
                    highLowColHeights.Item1 = height;
                }
                if (height > highLowColHeights.Item2) {
                    highLowCols.Item2 = col;
                    highLowColHeights.Item2 = height;
                }
            }
        }

        /// <summary>
        /// Tries one of the piece placements.
        /// Broken up intoa  seperate job for each to allow CPU workers to do other stuff during the calculation
        /// </summry>
        public void Execute() {
            
            var difference = highLowColHeights.Item2 - highLowColHeights.Item1;

            // If the highest and lowest columns are more than 5 rows apart or highest row is above row 6, 
            // target the lowest column
            if (difference > 5 || boardHighestRow > 6) {
                PlaceInColumn(highLowCols.Item2);
            } 
            
            // if top and bottom cols are 5 or less apart, safe enough to check all columns for best placement
            else {
                for (int i=rngConstant; i<rngConstant+32; i++) {
                    int col = (i/4)%8;
                    int rot = i%4;
                    SimulatePlacement(col, rot);
                }
            }

            // If no placements were found (bestPlacementRow is 0 which is impossible),
            // target the lowest column with a random rotation, ignore accuracy check
            if (bestPlacement[3] == 0) {
                // Debug.Log("no placements recognized - defaulting to lowest col");
                accuracy = 1.0f;

                SimulatePlacement(highLowCols.Item2, rngConstant%4, force: true);

                // if somehow there still isn't a placement, probably a rotation issue; flip the piece around
                if (bestPlacement[3] == 0) SimulatePlacement(highLowCols.Item2, (rngConstant%4) + 2, force: true);

                if (bestPlacement[3] == 0) Debug.LogWarning("Backup piece placements both failed");

                // if NO placements worked, check all placements that accuracy check skipped earlier,
                // as it may have skipped all valid placements
                for (int i=rngConstant; i<rngConstant+32; i++) {
                    int col = (i/4)%8;
                    int rot = i%4;
                    SimulatePlacement(col, rot, inverseAccuracy: true);
                }

                // when reaching here, there is no possible placements... so AI is dead,
                // no point in even trying to find a placement anymore
            }
        }

        void PlaceInColumn(int col) {    
            int rng = rngConstant%4;
            for (int i = rng; i < rng+4; i++) {
                int rot = i%4;
                SimulatePlacement(col, rot);
            }
        }

        /// <summary>
        /// Simulates placing the virtual piece at the designated location and orientation.
        /// </summary>
        /// <param name="col">column of the piece's center</param>
        /// <param name="rot">rotation of the piece. 0=up 1=right 2=down 3=left</param>
        /// <param name="force">if this should try to find a placement regardless of accuracy</param>
        void SimulatePlacement(int col, int rot, bool force = false, bool inverseAccuracy = false) {
            // If accuracy check for this placement failed, ai did not "see" this placement possibility
            if ( !force && !accuracyRng[(col*8 + rot) % 32] && !inverseAccuracy ) return;

            if (force) Debug.Log("Trying force placement: "+col+", "+rot);
            // else Debug.Log("Trying placement: "+col+", "+rot);

            // A list of mana colors with their positions as opposed to a full array,
            // since this will only be holding 3 tiles
            var virtualTiles = new NativeArray<VirtualTile>(3, Allocator.Temp);

            invalidPlacement = false;
            manaGain = 0;
            tileIndex = 0;

            // Tiles should be placed in reverse order of row so gravity happens in the right order.
            switch (rot)
            {
                case 0: // up
                    // tiles[0] = new VirtualTile(row, col, center);
                    SpawnTile(col, center, virtualTiles); // row
                    // tiles[2] = new VirtualTile(row, col+1, right);
                    SpawnTile(col+1, right, virtualTiles); // row
                    // tiles[1] = new VirtualTile(row-1, col, top);
                    SpawnTile(col, top, virtualTiles); // row-1
                    break;
                case 1: // left
                    // tiles[0] = new VirtualTile(row, col, center);
                    SpawnTile(col, center, virtualTiles); // row
                    // tiles[1] = new VirtualTile(row, col-1, top);
                    SpawnTile(col-1, top, virtualTiles); // row
                    // tiles[2] = new VirtualTile(row-1, col, right);
                    SpawnTile(col, right, virtualTiles); // row-1
                    break;
                case 2: // down
                    // tiles[1] = new VirtualTile(row+1, col, top);
                    SpawnTile(col, top, virtualTiles); // row+1
                    // tiles[0] = new VirtualTile(row, col, center);
                    SpawnTile(col, center, virtualTiles); // row
                    // tiles[2] = new VirtualTile(row, col-1, right);
                    SpawnTile(col-1, right, virtualTiles); // row
                    break;
                case 3: // right
                    // tiles[2] = new VirtualTile(row+1, col, right);
                    SpawnTile(col, right, virtualTiles); // row+1
                    // tiles[0] = new VirtualTile(row, col, center);
                    SpawnTile(col, center, virtualTiles);
                    // tiles[1] = new VirtualTile(row, col+1, top);
                    SpawnTile(col+1, top, virtualTiles); // row
                    break;
            }

            // if piece was found to be in an invalid position, do not set stats
            if (invalidPlacement) {
                // Debug.Log(col+", "+rot+": Invalid");
                virtualTiles.Dispose();
                return;
            }
 
            // Will check virtual tiles for connected mana and increment manaGAin accordingly
            CheckManaConnections(virtualTiles);


            int highestRow = 18;
            for (int v=0; v<3; v++) {
                highestRow = Mathf.Min(virtualTiles[v].row, highestRow);
            }

            // New placement must either net more mana, or net the same mana but have a lower highest tile position
            if (force || highestRow > bestPlacement[3] || (highestRow == bestPlacement[3] && manaGain > bestPlacement[2])) {
                // If above row 8 and earns 0 mana, do not place here
                // Should keep it from killing itself more often
                if (force || manaGain > 0 || highestRow >= 8) {
                    bestPlacement[0] = col;
                    bestPlacement[1] = rot;
                    bestPlacement[2] = manaGain;
                    bestPlacement[3] = highestRow;
                }

                // possible optimization: stop when 3 is reached. not implementing for 2 reasons rn
                // - job system goes through all indexes, idk how to avoid that yet
                // - may make the algorithm less fully random
            }

            virtualTiles.Dispose();
        }

        // Place a tile into the passed virtual tile list.
        void SpawnTile(int col, ManaColor color, NativeArray<VirtualTile> tiles) 
        {
            if (invalidPlacement) return;

            // if this tile's column is outside the board, piece cannot be moved here, and invalidate all other pieces
            if (col < 0 || col >= GameBoard.width) {
                invalidPlacement = true;
                return;
            }

            // Move down until it collides with a tile
            // r represents current row; r+1 represents tile below
            // start at the spawn height of 2

            // possible optimization if needed: save column heights on start, to reduce the need to check
            // tiles in all rows of columns for gravity

            for (int r = 2; r < GameBoard.height; r++) {
                // place here if reached the bottom of the board
                if (r+1 == GameBoard.height) {
                    PlaceTile(tiles, r, col, color);
                    return;
                }

                // Check if there's a tile below on the board or in virtual tiles
                // Loop through all virtual tiles to check if it's here - shouldn't take long
                // as it's only 3 elements long
                bool virtualTileOccupied = false;
                for (int i = 0; i < tileIndex; i++) {
                    if (
                        // this virtual tile's row will be 0 on init, and at least 1 once placed & fallen
                        tiles[i].row > 2 
                        // check if this virtual tile is in row below
                        && tiles[i].row == r+1
                        // check if this virtual tile is in correct column
                        && tiles[i].col == col) 
                    {
                        virtualTileOccupied = true;
                        break;
                    }
                }

                // if a virtual tile was occupied or tile exists on the actual gameboard, 
                // place new tile above it
                if (virtualTileOccupied || boardTiles[(r+1)*GameBoard.width + col] != ManaColor.None) {
                    PlaceTile(tiles, r, col, color);
                    return;
                }
            }
        }

        // Place the tile onto the board. Increment mana gain if connected to the same color
        void PlaceTile(NativeArray<VirtualTile> virtualTiles, int row, int col, ManaColor color) 
        {
            // if the tile ended up in the kill zone, don't place here
            if ((col == 3 || col == 4) && row <= 4) {
                invalidPlacement = true;
                return;
            }

            // add the virtual tile
            virtualTiles[tileIndex] = new VirtualTile(row, col, color);
            tileIndex++;
        }

        /// <summary>
        /// Checks all tiles to see if they are connected to another board or virtual mana tile.
        /// For each connected mana tile,  <c>manaGain</c> is incremented by 1.
        /// </summary>
        void CheckManaConnections(NativeArray<VirtualTile> virtualTiles) {
            for (int i=0; i<3; i++) {
                var tile = virtualTiles[i];
                CheckColorMatch(virtualTiles, i, tile.row-1, tile.col, tile.color);
                CheckColorMatch(virtualTiles, i, tile.row+1, tile.col, tile.color);
                CheckColorMatch(virtualTiles, i, tile.row, tile.col-1, tile.color);
                CheckColorMatch(virtualTiles, i, tile.row, tile.col+1, tile.color);
            }
        }

        // Check the board's tiles and virutal tiles if the color is matched.
        bool CheckColorMatch(NativeArray<VirtualTile> virtualTiles, int index, int row, int col, ManaColor color) {
            if (row < 0 || row >= GameBoard.height || col < 0 || col >= GameBoard.width) return false;

            if (boardTiles[row * GameBoard.width + col] == color) {
                manaGain++;
                return true;
            }

            for (int i=0; i<3; i++) {
                if (i == index) continue; // do not check that this tile is this tile
                
                if (virtualTiles[i].row == row && virtualTiles[i].col == col && virtualTiles[i].color == color) {
                    manaGain++;
                    return true;
                }
            }

            return false;
        }

        public struct VirtualTile
        {
            public int row, col;
            public ManaColor color;

            public VirtualTile(int row, int col, ManaColor color) {
                this.row = row;
                this.col = col;
                this.color = color;
            }

            public override string ToString() {
                return "("+row+", "+col+")";
            }
        }
    }
}