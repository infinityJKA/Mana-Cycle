using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Battle.Cycle;
using Sound;

namespace Battle.Board {
    /// <summary>
    /// Uses the Piece functionality but has no top or right segment.
    /// Rotation functions won't do anything
    /// </summary>
    public class SinglePiece : Piece
    {
        public Battler.ActiveAbilityEffect effect;

        // image for Infinity's Iron Sword
        [SerializeField] public Image ironSwordImage;
        [SerializeField] private AudioClip ironSwordSFX;

        [SerializeField] private Sprite pyroBombSprite;

        public override bool IsRotatable {get {return false;}}

        
        private Vector3 OrientedDirection()
        {
            return Vector3.up;
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
            switch(effect)
            {
                case Battler.ActiveAbilityEffect.IronSword:
                    Debug.Log("Iron Sword effect");
                    SoundManager.Instance.PlaySound(ironSwordSFX);
                    IronSwordDestroyTileBelow(board);
                    break;
                case Battler.ActiveAbilityEffect.PyroBomb:
                    Debug.Log("Pyro Bomb effect");
                    PyroBombExplode(board);
                    break;
                default:
                    Debug.Log("default single piece fall");
                    break;
            }
        }

        public void MakeIronSword(GameBoard board)
        {
            effect = Battler.ActiveAbilityEffect.IronSword;
            center.image.gameObject.SetActive(false);
            ironSwordImage.gameObject.SetActive(true);
            center.onFallAnimComplete = () => IronSwordDestroyTileBelow(board);
        }

        // Destroy the tile below this tile and deal damage
        // Return true if the tile should try to fall again
        private void IronSwordDestroyTileBelow(GameBoard board)
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


        public void MakePyroBomb(GameBoard board)
        {
            effect = Battler.ActiveAbilityEffect.PyroBomb;
            center.image.sprite = pyroBombSprite;
        }

        private void PyroBombExplode(GameBoard board) {
            Debug.Log("pyro bomb explosion");
            // Destroy tiles in a 3x3 grid (including this piece's bomb tile, which is in the center)
            // exclude this tile initial count

            int manaCleared = -1;
            Debug.Log(row+", "+col);
            for (int r = row-1; r <= row+1; r++) {
                for (int c = col-1; c <= col+1; c++) {
                    Debug.Log(r+", "+c);
                    if (board.ClearTile(c, r)) manaCleared++;
                }
            }
            board.AllTileGravity();

            board.DealDamage(board.damagePerMana*manaCleared, center.transform.position, 0, 0);
        }
    }
}