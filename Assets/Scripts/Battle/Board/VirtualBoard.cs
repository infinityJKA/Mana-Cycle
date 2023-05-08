using UnityEngine;
using Battle.Cycle;
using System.Collections.Generic;

namespace Battle.Board {
    /// <summary>
    /// A static class that simulates GameBoards to determine piece placement that nets the most mana in blobs
    /// </summary>
    public class VirtualBoard {
        public static PiecePlacement GetBestPlacement(GameBoard board, Piece piece) {
            PiecePlacement bestPlacement = new PiecePlacement();

            ManaColor[,] tiles = MatchState(board);
            List<VirtualBlob> blobs = MatchBlobs(board);

            var row = 2;
            ManaColor center = piece.GetCenter().color;
            ManaColor top = piece.GetTop().color;
            ManaColor right = piece.GetRight().color;

            for (int col = 0; col < GameBoard.width; col++) {
                for (int rot = 0; rot < 4; rot++) {
                    var placementTiles = tiles;
                    var placementBlobs = new List<VirtualBlob>(blobs);

                    var tilesPlaced = new List<VirtualTile>();

                    tilesPlaced[0] = new VirtualTile(row, col, center);

                    switch (rot)
                    {
                        case 0: // up
                            tilesPlaced[1] = new VirtualTile(row-1, col, top);
                            tilesPlaced[2] = new VirtualTile(row, col+1, right);
                            break;
                        case 1: // left
                            tilesPlaced[1] = new VirtualTile(row, col-1, top);
                            tilesPlaced[2] = new VirtualTile(row-1, col, right);
                            break;
                        case 2: // down
                            tilesPlaced[1] = new VirtualTile(row+1, col, top);
                            tilesPlaced[2] = new VirtualTile(row, col-1, right);
                            break;
                        case 3: // right
                            tilesPlaced[1] = new VirtualTile(row, col+1, top);
                            tilesPlaced[2] = new VirtualTile(row+1, col, right);
                            break;
                    }

                    // sort reverse by column - lower tiles will fall first
                    tilesPlaced.Sort((tile1, tile2) => tile1.col - tile2.col);

                    for (int i=0; i<3; i++) {
                        var tile = tilesPlaced[i];
                        for (int r=tile.row; r>=0; r--) {
                            if (placementTiles[r-1, tile.col] != ManaColor.Purple) {

                            }
                        }
                    }
                }
            }

            return bestPlacement;
        }

        /// <summary>
        /// Represents one of the ways a piece can be oriented, and optionally the amount of net mana gain it earns
        /// </summary>
        public struct PiecePlacement {
            public int column, rotation, manaGain;
        }

        /// <summary>
        /// Contains a list of all tiles in a blob, and the color.
        /// </summary>
        struct VirtualBlob
        {
            public ManaColor color;
            public List<Vector2Int> tiles;

            public VirtualBlob(GameBoard.Blob blob) {
                color = blob.color;
                tiles = new List<Vector2Int>(blob.tiles);
            }
        }

        struct VirtualTile
        {
            public int row, col;
            public ManaColor color;

            public VirtualTile(int row, int col, ManaColor color) {
                this.row = row;
                this.col = col;
                this.color = color;
            }
        }

        private static ManaColor[,] MatchState(GameBoard board) {
            ManaColor[,] tiles = new ManaColor[GameBoard.height, GameBoard.width];
            for (int r = 0; r < GameBoard.height; r++) {
                for (int c = 0; c < GameBoard.height; c++) {
                    tiles[r, c] = board.tiles[r, c].color;
                }
            }
            return tiles;
        }

        private static List<VirtualBlob> MatchBlobs(GameBoard board) {
            var blobs = new List<VirtualBlob>();
            foreach (GameBoard.Blob blob in board.blobs) {
                blobs.Add(new VirtualBlob(blob));
            }
            return blobs;
        }
    }
}