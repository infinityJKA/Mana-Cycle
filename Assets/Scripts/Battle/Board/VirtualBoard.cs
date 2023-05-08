// using UnityEngine;
// using Unity.Jobs;
// using UnityEngine.Jobs;
// using Unity.Collections;
// using Battle.Cycle;
// using System.Collections.Generic;

// namespace Battle.Board {
//     /// <summary>
//     /// A static class that simulates GameBoards to determine piece placement that nets the most mana in blobs
//     /// </summary>
//     public class VirtualBoard {
//         public static PiecePlacement GetBestPlacement(GameBoard board) {
//             var bestPlacements = new NativeList<PiecePlacement>();

//             ManaColor[,] tiles = MatchState(board);
//             NativeList<VirtualBlob> blobs = MatchBlobs(board);

//             var row = 2;
//             Piece piece = board.GetPiece();
//             ManaColor center = piece.GetCenter().color;
//             ManaColor top = piece.GetTop().color;
//             ManaColor right = piece.GetRight().color;

//             for (int col = 0; col < GameBoard.width; col++) {
//                 for (int rot = 0; rot < 4; rot++) {
//                     var placementTiles = tiles;
//                     var placementBlobs = new NativeList<VirtualBlob>(blobs);

//                     var tilesPlaced = new VirtualTile[3];

//                     tilesPlaced[0] = new VirtualTile(row, col, center);

//                     switch (rot)
//                     {
//                         case 0: // up
//                             tilesPlaced[1] = new VirtualTile(row-1, col, top);
//                             tilesPlaced[2] = new VirtualTile(row, col+1, right);
//                             break;
//                         case 1: // left
//                             tilesPlaced[1] = new VirtualTile(row, col-1, top);
//                             tilesPlaced[2] = new VirtualTile(row-1, col, right);
//                             break;
//                         case 2: // down
//                             tilesPlaced[1] = new VirtualTile(row+1, col, top);
//                             tilesPlaced[2] = new VirtualTile(row, col-1, right);
//                             break;
//                         case 3: // right
//                             tilesPlaced[1] = new VirtualTile(row, col+1, top);
//                             tilesPlaced[2] = new VirtualTile(row+1, col, right);
//                             break;
//                     }

//                     // check all tiles to make sure they are within the board; if not, skip this placement
//                     bool valid = true;
//                     foreach (var tile in tilesPlaced) {
//                         if (tile.row < 0 || tile.row >= GameBoard.height || tile.col < 0 || tile.col >= GameBoard.width) {
//                             valid = false;
//                             break;
//                         }
//                     }
//                     if (!valid) continue;

//                     // sort reverse by column - lower tiles will fall first
//                     System.Array.Sort(tilesPlaced, (tile1, tile2) => tile1.col - tile2.col);

//                     Debug.Log(tilesPlaced[0]+" - "+tilesPlaced[1]+" - "+tilesPlaced[2]);

//                     int netMana = 0;

//                     // For each tile, make it fall, and once it hits the ground or a tile, try to add it to a blob
//                     for (int i=0; i<3; i++) {
//                         var tile = tilesPlaced[i];
//                         for (int r=tile.row; r<GameBoard.height; r++) {
//                             Debug.Log(r+", "+tile.col);
//                             if (r+1 == GameBoard.height || placementTiles[r+1, tile.col] != ManaColor.None) {
//                                 placementTiles[r, tile.col] = tile.color;

//                                 Vector2Int tilePos = new Vector2Int(r, tile.col);
                                
//                                 // for each blob, 
//                                 // check that the blob matches the color,
//                                 // check if this tile lies within it or its 1 tile border, 
//                                 // and that tile is not already taken up in the board
//                                 foreach (var blob in placementBlobs) {
//                                     if (blob.color != tile.color) continue;
//                                     if (!blob.tiles.Contains(tilePos)) continue;
//                                     if (placementTiles[r, tile.col] != ManaColor.None) continue;

//                                     // if all tests passed, add 1 to mana and place the tile there
//                                     netMana++;
//                                     placementTiles[r, tile.col] = tile.color;
//                                     blob.Expand(tilePos);
//                                     break;
//                                 }
//                                 break;
//                             }
//                         }
//                     }

//                     // if net mana exceeds highest existing, clear the list
//                     if (bestPlacements.Length > 0 && netMana > bestPlacements[0].manaGain) {
//                         bestPlacements.Clear();
//                     }
//                     // add the placement to the list, which will be the only item if just cleared
//                     bestPlacements.Add(new PiecePlacement(col, rot, netMana));
//                 }
//             }

//             // finally, return a random placement with the maximum mana gain
//             return bestPlacements[Random.Range(0, bestPlacements.Length)];
//         }

//         /// <summary>
//         /// Represents one of the ways a piece can be oriented, and optionally the amount of net mana gain it earns
//         /// </summary>
//         public struct PiecePlacement {
//             public int column, rotation, manaGain;

//             public PiecePlacement(int column, int rotation, int manaGain) {
//                 this.column = column;
//                 this.rotation = rotation;
//                 this.manaGain = manaGain;
//             }
//         }

//         /// <summary>
//         /// Contains a list of all tiles in a blob, and the color.
//         /// The edges are expanded out by 1 on all tiles, so the code can more effeciently check
//         /// if a tile lies within the blob.
//         /// </summary>
//         readonly struct VirtualBlob
//         {
//             public readonly ManaColor color;
//             public readonly NativeHashSet<Vector2Int> tiles;

//             public VirtualBlob(GameBoard.Blob blob) {
//                 color = blob.color;
//                 tiles = new NativeHashSet<Vector2Int>();

//                 foreach (var tile in blob.tiles) {
//                     Expand(tile);
//                 }
//             }

//             public void Expand(Vector2Int tile) {
//                 tiles.Add(new Vector2Int(tile.x, tile.y));
//                 tiles.Add(new Vector2Int(tile.x-1, tile.y));
//                 tiles.Add(new Vector2Int(tile.x+1, tile.y));
//                 tiles.Add(new Vector2Int(tile.x, tile.y-1));
//                 tiles.Add(new Vector2Int(tile.x, tile.y+1));
//             }
//         }

//         struct VirtualTile
//         {
//             public int row, col;
//             public ManaColor color;

//             public VirtualTile(int row, int col, ManaColor color) {
//                 this.row = row;
//                 this.col = col;
//                 this.color = color;
//             }

//             public override string ToString() {
//                 return "("+row+", "+col+")";
//             }
//         }

//         private static ManaColor[,] MatchState(GameBoard board) {
//             ManaColor[,] tiles = new ManaColor[GameBoard.height, GameBoard.width];
//             for (int r = 0; r < GameBoard.height; r++) {
//                 for (int c = 0; c < GameBoard.width; c++) {
//                     tiles[r, c] = board.tiles[r, c] ? board.tiles[r, c].color : ManaColor.None;
//                 }
//             }
//             return tiles;
//         }

//         private static NativeList<VirtualBlob> MatchBlobs(GameBoard board) {
//             var blobs = new NativeList<VirtualBlob>();
//             foreach (GameBoard.Blob blob in board.blobs) {
//                 blobs.Add(new VirtualBlob(blob));
//             }
//             return blobs;
//         }
//     }


//     public struct FindBestPlacementJob : IJobParallelFor
//     {
//         public void Execute(int index) {
            
//         }
//     }
// }