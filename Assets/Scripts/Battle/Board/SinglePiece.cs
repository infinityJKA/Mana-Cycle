using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
        // image for Infinity's Iron Sword
        [SerializeField] public Image ironSwordImage;
        [SerializeField] private AudioClip ironSwordSFX;
        [SerializeField] private AudioClip pyroBombSFX;

        [SerializeField] private Sprite pyroBombSprite;

        public override bool IsRotatable {get {return false;}}

        [SerializeField] private GameObject goldMineObject;

        [SerializeField] private Sprite zmanSprite;

        
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
            // Debug.Log("using SinglePiece iterator");
            // Return the only tile, the center tile
            yield return new Vector2Int(col, row);
        }

        // Place this tile's pieces onto the passed board.
        public override void PlaceTilesOnBoard(ref Tile[,] board, Transform pieceBoard)
        {
            if (!center) {
                Debug.LogWarning("trying to place destroyed tile");
                return;
            }
            // Place this single piece's tile and set its parent
            board[row, col] = center;

            center.transform.SetParent(pieceBoard, true);
            center.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        public override void DestroyTiles() {
            Destroy(center);
        }

        public override void MakeGhostPiece(ref List<Tile> ghostTiles) {
            ghostTiles.Add(center);
            
            if (effect == Battler.ActiveAbilityEffect.IronSword) {
                ironSwordImage.gameObject.SetActive(false);
                center.image.gameObject.SetActive(true);
            }

            else if (effect == Battler.ActiveAbilityEffect.GoldMine) {
                transform.GetComponentInChildren<MeshRenderer>().enabled = false;
            }
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
            slowFall = true;
            center.DontDoGravity();
            center.SetColor(ManaColor.Colorless, board, false, false);
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
                board.ClearTile(col, row-1, doParticleEffects: false);
                return;
            }
            // When iron sword falls, clear tile below, or destroy when at bottom
            board.DealDamage((int)(board.damagePerMana*2.5), center.transform.position, 0, 0);
            board.ClearTile(col, row);
            board.TileGravity(col, row-1, force: true); // makes this piece's tile fall
            // may cause ta tile to not be in a valid clearing blob - check to unglow them
            board.UnglowNotInBlobs();
        }


        public void MakePyroBomb(GameBoard board)
        {
            effect = Battler.ActiveAbilityEffect.PyroBomb;
            center.SetColor(ManaColor.Colorless, board, false, false);
            center.image.sprite = pyroBombSprite;
            center.SetVisualColor(Color.white);
            center.image.gameObject.SetActive(true);
        }

        private void PyroBombExplode(GameBoard board) {
            Debug.Log("pyro bomb explosion");
            SoundManager.Instance.PlaySound(pyroBombSFX);
            
            // Destroy tiles in a 3x3 grid (including this piece's bomb tile, which is in the center)
            // exclude this tile initial count

            var explosionCenter = center.transform.position; // grab this before tile is destroyed
            float totalPointMult = 0;
            // Debug.Log(row+", "+col);
            for (int r = row-1; r <= row+1; r++) {
                for (int c = col-1; c <= col+1; c++) {
                    // Debug.Log(r+", "+c);
                    totalPointMult += board.ClearTile(c, r);
                }
            }
            board.AllTileGravity();

            // Because this may cause a tile to fall outside of a blob, unglow un blob tiles
            board.UnglowNotInBlobs();

            board.DealDamage((int)(board.damagePerMana*totalPointMult*2f), explosionCenter, 0, 0);
        }

        
        public void MakeGoldMine(GameBoard board) {
            Debug.Log("gold mine piece creation");

            effect = Battler.ActiveAbilityEffect.GoldMine;

            // // (Old) Tile color mirrors the center color of the current piece it is replacing
            // center.SetColor(board.GetPiece().GetCenter().color, board);

            // (New) tile is always multicolor
            center.SetColor(ManaColor.Multicolor, board);
            // make tile semi transparent
            center.SetVisualColor(new Color(center.image.color.r, center.image.color.g, center.image.color.b, 0.5f));

            // This tile's point mult should be 0, unless another mana somehow buffs it
            center.pointMultiplier -= 1.00f;

            // Before this tile is cleared, add a +200% point multiplier to all connected mana
            // (Don't buff this mana, it should stay at 0)
            center.beforeClear = (blob) => {
                foreach (var tilePos in blob.tiles) {
                    if (tilePos.y == row && tilePos.x == col) return;
                    board.tiles[tilePos.y, tilePos.x].pointMultiplier += 2.00f;
                }
            };

            // instantiate the crystal object and move it away from the camera, but not beyond the board
            GameObject crystal = Instantiate(
                goldMineObject, 
                center.image.transform.position + Vector3.forward*2, 
                Quaternion.identity, 
                center.image.transform
            ); 

            // set material to cycle's corresponding crystal material            
            crystal.GetComponent<MeshRenderer>().material = board.cycle.crystalMaterials[(int)center.color];
        }

        public void MakeZman(GameBoard board) {
            center.SetColor(ManaColor.Colorless, board);
            center.image.sprite = zmanSprite;
            center.MakeObscuresColor();
            center.MakeFragile();
            center.pointMultiplier -= 1.0f;
        }
    }
}