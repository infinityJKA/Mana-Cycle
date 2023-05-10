using System;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Battle.Cycle;
using System.Collections.Generic;

namespace Battle.Board {
    public struct BestPlacementJob : IJob
    {
        // ---- Inputs
        // This is a flattened 2D array so that it fits the native array.
        // Coordinate system works like this: array[row * width + col]
        public NativeArray<ManaColor> boardTiles;

        // // Minimum and maximum column.
        // // If the AI cannot reach a column when moving left/right,
        // // it will restrict itself from trying to go to that column until the next clear
        // public int minCol, maxCol;
        
        // public NativeArray<VirtualTile> virtualTiles;

        // inner
        int rngConstant;

        ManaColor center, top, right;
        int tileIndex;

        // stores if the current placement is invalid
        bool invalidPlacement;

        // ---- Outputs
        // Has 3 variables for the best placement position found.
        // [bestCol, bestRot, bestManaGain]
        public NativeArray<int> bestPlacement;

        public int manaGain;

        public BestPlacementJob(GameBoard board, NativeArray<int> bestPlacement, int preferredColumn = -1) {
            this.bestPlacement = bestPlacement;

            // copy down the state of the board's tiles' colors
            boardTiles = new NativeArray<ManaColor>(GameBoard.height*GameBoard.width, Allocator.Persistent);
            for (int r = 0; r < GameBoard.height; r++) {
                for (int c = 0; c < GameBoard.width; c++) {
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
            // that might also earn the max 3 mana gain instead of all left/right
            rngConstant = UnityEngine.Random.Range(0, 32);
        }


        // Tries one of the piece placements.
        // Broken up intoa  seperate job for each to allow CPU workers to do other stuff during the calculation
        public void Execute() {
            var cols = HighestAndLowestColumns();
            var difference = cols.Item2 - cols.Item1;

            // If the highest and lowest columns are more than 5 rows apart, target the lowest column
            if (difference > 5) {
                int col = cols.Item2;
                
                int rng = rngConstant%4;
                for (int i = rng; i < rng+4; i++) {
                    int rot = i%4;
                    SimulatePlacement(col, rot);
                }
            } 
            
            // if top and bottom cols are 5 or less apart, safe enough to check all columns for best placement
            else {
                for (int i=rngConstant; i<rngConstant+32; i++) {
                    int col = (i/4)%8;
                    int rot = i%4;
                    SimulatePlacement(col, rot);
                }
            }
        }

        void SimulatePlacement(int col, int rot) {
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

            int highestRow = 18;
            for (int v=0; v<3; v++) {
                highestRow = Mathf.Min(virtualTiles[v].row, highestRow);
            }

            // // If any of the tiles are above row 6, ignore this placement, too dangerous
            // if (highestRow < 6) continue;

            if (highestRow >= bestPlacement[3]) {
                // if highestRow is the same, manaGain is the tiebreaker.
                // if manaGain is lower, do not consider this placement
                if (highestRow == bestPlacement[3] && manaGain < bestPlacement[2]) {
                    return;
                }

                // If above row 8 and earns 0 mana, do not place here
                // Should keep it from killing itself more often
                if (manaGain > 1 || highestRow >= 8) {
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
            // add the virtual tile
            virtualTiles[tileIndex] = new VirtualTile(row, col, color);

            // check all adjacent board and virtual tiles
            if (
                MatchesColor(virtualTiles, row-1, col, color)
                || MatchesColor(virtualTiles, row+1, col, color)
                || MatchesColor(virtualTiles, row, col-1, color)
                || MatchesColor(virtualTiles, row, col+1, color)
            ) {
                tileIndex++;
                manaGain++;
            } else {
                tileIndex++;
            }
        }

        // Check the board's tiles and virutal tiles if the color is matched.
        bool MatchesColor(NativeArray<VirtualTile> virtualTiles, int row, int col, ManaColor color) {
            if (row < 0 || row >= GameBoard.height || col < 0 || col >= GameBoard.width) return false;

            if (boardTiles[row * GameBoard.width + col] == color) return true;

            for (int i=0; i<tileIndex; i++) {
                if (virtualTiles[i].row == row && virtualTiles[i].col == col && virtualTiles[i].color == color) return true;
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

        // Return the column numbers for the hgihest and lowest columns.
        // Start with a random offset, to include more randomness
        (int, int) HighestAndLowestColumns() {
            // first item is highest column (lowest row value)
            // second item is lowest column (highest row value)
            var heights = (18, 0);
            var columns = (-1, -1);

            int offset = rngConstant%8;
            for (int c = offset; c < GameBoard.width+offset; c++) {
                var height = ColumnHeight(c%8);

                if (height < heights.Item1) {
                    heights.Item1 = height;
                    columns.Item1 = c;
                }
                if (height > heights.Item2) {
                    heights.Item2 = height;
                    columns.Item2 = c;
                }
            }

            return columns;
        }

        // Return the row height of the tallest column.
        int ColumnHeight(int col) {
            for (int row = GameBoard.height - 1; row >= 0; row--) {
                if (boardTiles[row*GameBoard.width + col] == ManaColor.None) {
                    return row;
                }
            }
            return 0;
        }
    }
}