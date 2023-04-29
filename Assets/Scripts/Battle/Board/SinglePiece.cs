using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Battle.Cycle;

namespace Battle.Board {
    /// <summary>
    /// Uses the Piece functionality but has no top or right segment.
    /// Rotation functions won't do anything
    /// </summary>
    public class SinglePiece : Piece
    {
        public SinglePieceMode mode;
        public enum SinglePieceMode {
            SingleMana,
            IronSword
        }

        // image for Infinity's Iron Sword
        [SerializeField] public Image ironSwordImage;
        
        private Vector3 OrientedDirection()
        {
            return Vector3.up;
        }

        // Randomize the color of the tiles of this piece.
        public override void Randomize(GameBoard board)
        {
            PieceRng rng = board.GetPieceRng();

            if (rng == PieceRng.CurrentColorWeighted)
            {
                center.SetColor(ColorWeightedRandom(board), board);
            }

            else if (rng == PieceRng.PureRandom)
            {
                center.SetColor(RandomColor(), board);
            }

            else if (rng == PieceRng.Bag)
            {
                center.SetColor(pullColor(), board);
            }
        }
            
        public override void UpdateOrientation()
        {
            // do nothing :)
        }

        public override IEnumerator<Vector2Int> GetEnumerator()
        {
            Debug.Log("using SinglePiece iterator");
            // Return the only tile, the center tile
            yield return new Vector2Int(col, row);
        }

        // Place this tile's pieces onto the passed board.
        public override void PlaceTilesOnBoard(ref Tile[,] board, Transform pieceBoard)
        {
            // Place this single piece's tile and set its parent
            board[row, col] = center;

            center.transform.SetParent(pieceBoard, true);
            center.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        public override void DestroyTiles() {
            Destroy(center);
        }

        public override void OnPlace(GameBoard board) 
        {
            switch(mode)
            {
                case SinglePieceMode.IronSword:
                    Debug.Log("Iron Sword effect");
                    IronSwordDestroyTileBelow(board);
                    break;
                default:
                    Debug.Log("default single piece fall");
                    break;
            }
        }

        // Destroy the tile below this tile and deal damage
        // Return true if the tile should try to fall again
        public void IronSwordDestroyTileBelow(GameBoard board)
        {
            row++;
            if (row >= GameBoard.height) {
                Destroy(board.tiles[row-1, col].gameObject);
                return;
            }
            // When iron sword falls, clear tile below, or destroy when at bottom
            Tile swordTile = board.tiles[row-1, col];
            board.ClearTile(col, row);
            board.TileGravity(col, row-1);
            board.DealDamage(board.damagePerMana, swordTile.transform.position, 0, 0);
        }

        public void MakeIronSword(GameBoard board)
        {
            mode = SinglePieceMode.IronSword;
            center.imageObject.SetActive(false);
            ironSwordImage.gameObject.SetActive(true);
            center.onFallAnimComplete = () => IronSwordDestroyTileBelow(board);
        }
    }
}