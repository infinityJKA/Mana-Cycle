using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Battle.Cycle;

namespace Battle.Board {
    public class Tile : MonoBehaviour
    {
        // If within a piece, the row and column relative to that piece, before rotation orientation.
        // If placed on the board, the row and column of this tile on the grid.
        public int row;
        public int col;

        // Mana color int representation value for this tile (changed from ManaColor enum -> int)
        public int manaColor { get; private set; }

        // Point multiplier when this tile is cleared. May be modified by abilities.
        public float pointMultiplier {get; set;} = 1f;

        // Runs right before this tile is cleared. If part of a blob, that blob is passed.
        public Action<GameBoard.Blob> beforeClear {get; set;}

        // If this is a trash tile - which damages in set intervals
        public bool trashTile { get; private set; }

        // Whether or not this tile obscures mana colors around it. (zman)
        public bool obscuresColor { get; private set; }

        // Whether or not this tile's color is currently obscured.
        public bool obscured { get; private set; }

        // Fragile tiles are only cleared when an adjacent blob is cleared.
        public bool fragile { get; private set; }

        // Duration left before this tile destroys itself - ticks down if set to above 0
        public float lifespan {get; private set;}
        private float lifeStart;

        // If gravity should pull this tile down.
        public bool doGravity { get; private set; } = true;

        /// <summary>internal bool used by GameBoard when lighting connected tiles</summary>
        public bool connectedToGhostPiece {get; set;}

        // -- Serialized
        [SerializeField] private TileVisual _visual;
        public TileVisual visual => _visual;

        public GameBoard board;
        public String specialProperty;
        private int InfernoCleared = 0;
        
        void Update(){
            if(lifespan != 0){
                if(specialProperty == "Inferno" && board.tiles[row-1,col] != null){
                    board.ClearTile(col,row-1);
                    if(board.Battler.activeAbilityEffect == Battler.ActiveAbilityEffect.Inferno){
                        board.DealDamageLocal(Convert.ToInt32(10+(1.8*InfernoCleared)), -1, transform.position);
                        InfernoCleared++;
                    }
                    board.AllTileGravity();
                }
                if(Time.time-lifespan >= lifeStart){
                    Debug.Log("SELF DESTRUCT MANA at COL "+col+"  ROW "+row);
                    board.ClearTile(col, row);
                    board.AllTileGravity();
                }
            }
        }


        public void SetManaColor(int manaColor, GameBoard board, bool setVisual = true, bool ghost = false)
        {
            this.manaColor = manaColor;

            if (setVisual && ManaCycle.instance.usingSprites) {
                if (ghost) {
                    _visual.SetGhostVisual(board, manaColor);
                } else {
                    _visual.SetVisual(board.cosmetics, manaColor, isTrash: trashTile);
                }
            }
        }

        public int GetManaColor()
        {
            return manaColor;
        }

        public void AnimateMovement(Vector2 from, Vector2 to) {
            _visual.AnimateMovement(from, to);
        }

        /// <summary>
        /// Runs the stored beforeClear method.
        /// Is run before the tiles are damage calculated and removed from the board.
        /// </summary>
        /// <param name="blob">the blob this is in, or null if not in a blob</param>
        public void BeforeClear(GameBoard.Blob blob) {
            if (beforeClear != null) beforeClear(blob);
        }

        public void MakeTrashTile() {
            trashTile = true;
            pointMultiplier -= 1.00f;
        }

        public void MakeObscuresColor() {
            obscuresColor = true;
        }

        public void DontDoGravity() {
            doGravity = false;
        }

        public void MakeFragile() {
            fragile = true;
        }

        public void SetLifespan(int n){
            lifeStart = Time.time;
            lifespan = n;
        }

        public void Obscure(GameBoard board) {
            // If this itself is an obscuring tile, do not obscure it
            if (obscuresColor) return;

            if (!obscured) {
                obscured = true;
                _visual.SetObscuredVisual(board);
            }
        }

        public void Unobscure(GameBoard board) {
            if (obscured) {
                obscured = false;
                SetManaColor(manaColor, board);
            }
        }
    }
}
