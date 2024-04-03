using UnityEngine;
using System.Collections.Generic;

using Battle.Cycle;
using System;

namespace Battle.Board {
    public class Piece : MonoBehaviour
    {
        // Theoretically, these should stay the same as this piece's gameobject position, but I don't trust floats.
        // Column position of this piece on the grid.
        [SerializeField] protected int col = 0;
        // Row position of this piece on the grid.
        [SerializeField] protected int row = 0;

        // Think of each piece as an L on your left hand. Top is pointer finger and right is thumb.
        // Tile in center
        [SerializeField] protected Tile[] tiles; 

        /// <summary>
        /// Rotation center - holds all the tile objects. Centered on tile for correct visual rotation.
        /// </summary>
        [SerializeField] protected Transform rotationCenter;

        /// <summary>
        /// This piece's rotation, direction that the top tile is facing. Start out facing up.
        /// </summary>
        [SerializeField] protected Orientation orientation = Orientation.up;

        /// <summary>
        /// If this is a ghost piece - tiles will not actually physically exist on the board
        /// </summary>
        public bool ghostPiece { get; private set; }

        /// <summary>
        /// This piece's ID. Assigned by gameboard. each piece within the board will have a unique ID counting up from 0.
        /// </summary>
        public int id = -1;

        /// <summary>
        /// Orientation is the way that the "top" tile is facing
        /// </summary>
        public enum Orientation
        {
            up,
            left,
            down,
            right
        }

        /// <summary>
        /// Special ability effect when this tile is placed. Used only by single pieces
        /// </summary>
        public Battler.ActiveAbilityEffect effect;

        public bool isRotatable {get; set;} = true;

        /// <summary>
        /// If this is a special tile that should fall much slower than other tiles - ex. Infinity's sword
        /// </summary>
        public bool slowFall { get; private set; }

        private Vector3 OrientedDirection()
        {
            switch(orientation)
            {
                case Orientation.up: return Vector3.up;
                case Orientation.left: return Vector3.left;
                case Orientation.down: return Vector3.down;
                case Orientation.right: return Vector3.right;
                default: return Vector3.zero;
            }
        }

        // Randomize the color of the tiles of this piece.
        public virtual void Randomize(GameBoard board)
        {
            PieceRng rng = board.GetPieceRng();

            if (rng == PieceRng.CurrentColorWeighted)
            {
                foreach (Tile tile in tiles) {
                    tile.SetManaColor(ColorWeightedRandom(board), board);
                } 
            }

            if (rng == PieceRng.PieceSameColorWeighted)
            {
                tiles[0].SetManaColor(RandomColor(board.rngManager.rng), board);

                // for top and right, 40% chance to mirror the center color
                for (int i = 1; i < tiles.Length; i++) {
                    if (board.rngManager.rng.NextDouble() < 0.4) {
                        tiles[i].SetManaColor(tiles[0].manaColor, board);
                    } else {
                        tiles[i].SetManaColor(RandomColor(board.rngManager.rng), board);
                    }
                }
            }

            else if (rng == PieceRng.PureRandom)
            {
                foreach (Tile tile in tiles) {
                    tile.SetManaColor(RandomColor(board.rngManager.rng), board);
                } 
            }

            else if (rng == PieceRng.Bag)
            {
                // pull color from randomized bag
                foreach (Tile tile in tiles) {
                    tile.SetManaColor(board.rngManager.PullColorFromBag(), board);
                }
            }

            else if (rng == PieceRng.CenterMatchesCycle)
            {
                // first tile (center) matches cycle, all others are random
                tiles[0].SetManaColor(board.rngManager.GetCenterMatch(), board);
                for (int i = 1; i < tiles.Length; i++) {
                    tiles[i].SetManaColor(RandomColor(board.rngManager.rng), board);
                }
            }
        }

        // uses a System.Random seeded instance
        public static int RandomColor(System.Random rng)
        {
            return rng.Next(0, ManaCycle.lockPieceColors ? ManaCycle.cycleUniqueColors : 5);
        }

        // uses built in Unity random - not seeded! shouldnt be used in online
        // public static int RandomColor()
        // {
        //     return UnityEngine.Random.Range(0, ManaCycle.lockPieceColors ? ManaCycle.cycleUniqueColors : 5);
        // }

        protected static int ColorWeightedRandom(GameBoard board)
        {
            // still always pull from bag, but replace some with cycle color
            var bagColor = board.rngManager.PullColorFromBag();
            if (board.rngManager.rng.NextDouble() < 0.15)
            {
                return board.GetCycleColor();
            } else {
                return bagColor;
            }
        }

        // Translate this piece by the given X and Y.
        public void Move(int col, int row)
        {
            this.col += col;
            this.row += row;
            // Update local position, Add half width and height cause it isnt working without that idk why.
            UpdatePosition();
            
        }
        public void MoveTo(int col, int row)
        {
            this.col = col;
            this.row = row;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            if (!this) {
                Debug.LogError("Trying to move a destroyed piece");
                return;
            }
            transform.localPosition = new Vector3(this.col - GameBoard.width/2f, -this.row + GameBoard.physicalHeight/2f + GameBoard.height - GameBoard.physicalHeight, 0);
        }
            
        // Rotate this piece to the right about the center.
        public void RotateRight()
        {
            if (!isRotatable) {
                Debug.LogWarning("Trying to rotate an unrotatable piece");
                return;
            }

            switch (orientation)
            {
                case Orientation.up:
                    orientation = Orientation.right; break;
                case Orientation.right:
                    orientation = Orientation.down; break;
                case Orientation.down:
                    orientation = Orientation.left; break;
                case Orientation.left:
                    orientation = Orientation.up; break;
            }
            UpdateOrientation();
        }

        // Rotate this piece to the left about the center.
        public void RotateLeft()
        {
            if (!isRotatable) {
                Debug.LogWarning("Trying to rotate an unrotatable piece");
                return;
            }
            
            switch (orientation)
            {
                case Orientation.up:
                    orientation = Orientation.left; break;
                case Orientation.left:
                    orientation = Orientation.down; break;
                case Orientation.down:
                    orientation = Orientation.right; break;
                case Orientation.right:
                    orientation = Orientation.up; break;
            }
            UpdateOrientation();
        }

        public void SetRotation(Orientation orientation) {
            if (!this) {
                Debug.LogError("Trying to rotate a destroyed piece");
                return;
            }
            this.orientation = orientation;
            UpdateOrientation();
        }

        // Update the roation of this object's rotation center, after orientation changes.
        public virtual void UpdateOrientation()
        {
            if (!isRotatable || !rotationCenter) {
                Debug.LogWarning("Trying to update orientation of unrotatable title");
                return;
            }
            rotationCenter.rotation = Quaternion.LookRotation(Vector3.forward, OrientedDirection());

            // make the inner tiles face opposite rotation, so animation stays correct
            // var opposite = UndoOrientedDirection();
            foreach (Tile tile in tiles) {
                tile.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            }
        }

        // Iteration of all coordinates this piece currently occupies. Returns Vector2Ints of (col, row).
        public virtual IEnumerator<Vector2Int> GetEnumerator()
        {
            foreach (Tile tile in tiles) {
                yield return PieceToBoardPos(tile);
            }
        }

        // Place this tile's pieces onto the passed board.
        public virtual void PlaceTilesOnBoard(ref Tile[,] tileGrid, Transform pieceBoard)
        {
            foreach (Tile tile in tiles) {
                Vector2Int boardPos = PieceToBoardPos(tile);
                tileGrid[boardPos.y, boardPos.x] = tile;

                tile.transform.SetParent(pieceBoard, true);
                tile.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            }
        }

        public virtual void DestroyTiles() {
            foreach (Tile tile in tiles) {
                Destroy(tile.gameObject);
            }
        }

        public virtual void MakeGhostPiece(GameBoard board, ref List<Tile> ghostTiles) {
            foreach (Tile tile in tiles) {
                ghostTiles.Add(tile);
            }

            if (effect == Battler.ActiveAbilityEffect.IronSword) {
                // undo scale and offset changes
                center.SetManaColor(ManaColor.None, board, ghost: true);
                center.visual.SetSizeDelta(Vector2.one);
                center.visual.SetAnchoredPosition(Vector2.zero);
            }

            else if (effect == Battler.ActiveAbilityEffect.GoldMine) {
                transform.GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }

        // Accessors
        public int GetRow()
        {
            return row;
        }

        public int GetCol()
        {
            return col;
        }

        public Orientation GetRotation(){
            return orientation;
        }

        public Tile GetTile(int index) {
            return tiles[index];
        }

        public Tile center => tiles[0];

        public Tile[] GetTiles() {
            return tiles;
        }

        public Vector2Int TilePos(int index) {
            return PieceToBoardPos(tiles[index]);
        }

        public int tileCount => tiles.Length;

        /// <summary>
        /// Convert the position of a tile within this piece to board position (undo the rotation).
        /// </summary>
        /// <param name="piecePosition">row-col position relative to this piece's center</param>
        /// <returns>position of the tile on the board in format (col, row)</returns>
        public Vector2Int PieceToBoardPos(Tile tile) {
            switch (orientation)
            {
                case Orientation.up:
                    return new Vector2Int(col + tile.col, row + tile.row);
                case Orientation.left:
                    return new Vector2Int(col + tile.row, row - tile.col);
                case Orientation.down:
                    return new Vector2Int(col - tile.col, row - tile.row);
                case Orientation.right:
                    return new Vector2Int(col - tile.row, row + tile.col);
                default:
                    Debug.LogWarning("Invalid piece orientation somehow");
                    return Vector2Int.zero;
            }
        }

        // ======== ABILITY MANAGEMENT
            /// <summary>
        /// Called when this piece is placed
        /// (used in active abilities)
        /// </summary>
        public virtual void OnPlace(GameBoard board)
        {  
            switch(effect)
            {
                case Battler.ActiveAbilityEffect.IronSword:
                    // Debug.Log("Iron Sword effect");
                    Instantiate(board.cosmetics.ironSwordSFX);
                    IronSwordDestroyTileBelow(board);
                    break;
                case Battler.ActiveAbilityEffect.PyroBomb:
                    // Debug.Log("Pyro Bomb effect");
                    PyroBombExplode(board);
                    break;
                default:
                    // Debug.Log("default single piece fall");
                    break;
            }
        }

        // ======== ABILITY PIECE CREATION
        public void MakeIronSword(GameBoard board)
        {
            effect = Battler.ActiveAbilityEffect.IronSword;
            slowFall = true;
            center.DontDoGravity();
            center.SetManaColor(ManaColor.Colorless, board, setVisual: false);
            center.visual.SetSprite(board.cosmetics.ironSwordSprite);
            center.visual.SetColor(Color.white);
            center.visual.SetSizeDelta(new Vector2(1, 2));
            center.visual.SetAnchoredPosition(new Vector2(0, 0.5f)); // aligns the bottom of the 2 tile large image to the bottom of the single tile

            accumulatedDamage = 0;
            center.visual.onFallAnimComplete = () => IronSwordDestroyTileBelow(board);
        }

        // Destroy the tile below this tile and deal damage
        // Return true if the tile should try to fall again
        int accumulatedDamage;
        private void IronSwordDestroyTileBelow(GameBoard board)
        {
            row++;
            // Removes this tile when it reaches the bottom of the board.
            if (row >= GameBoard.height) {
                board.ClearTile(col, row-1, doParticleEffects: false);

                // in online only, wait until after hitting the ground, and then send damage to other client
                // in case there was a desync due to the very many damage instances in a short burst
                if (Storage.online && board.netPlayer.isOwned) {
                    board.DealDamage(accumulatedDamage, center.transform.position, partOfChain: false);
                }
            }
            // only locally evaluate damage if either not online or player owns this client and not the opponent
            int damage = (int)(board.damagePerMana*2.5);
            if (!Storage.online || board.netPlayer.isOwned) {
                board.DealDamageLocal(damage, center.transform.position);
            } else {
                accumulatedDamage += damage;
            }

            // When iron sword falls, clear tile below, or destroy when at bottom
            board.ClearTile(col, row);
            board.TileGravity(col, row-1, force: true); // makes this piece's tile fall

            // may cause ta tile to not be in a valid clearing blob - check to unglow them
            board.UnglowNotInBlobs();
        }


        public void MakePyroBomb(GameBoard board)
        {
            effect = Battler.ActiveAbilityEffect.PyroBomb;
            center.SetManaColor(ManaColor.Colorless, board, setVisual: false);
            center.visual.SetSprite(board.cosmetics.pyroBombSprite);
            center.visual.SetColor(Color.white);
        }

        private void PyroBombExplode(GameBoard board) {
            Debug.Log("pyro bomb explosion");
            Instantiate(board.cosmetics.pyroBombSFX);

            
            // Destroy tiles in a 5x5 grid (including this piece's bomb tile, which is in the center)
            // exclude this tile initial count

            var explosionCenter = center.transform.position; // grab this before tile is destroyed
            board.SpawnParticles(row, col, board.cosmetics.pyroBombParticleEffect, new Vector3(0, 0, 2));

            float totalPointMult = 0;
            for (int r = row-2; r <= row+2; r++) {
                for (int c = col-2; c <= col+2; c++) {
                    // White clear particles around border to emphasize 5x5 shape
                    // if (Math.Abs(r-row) == 2 || Math.Abs(c-col) == 2) {
                    //     board.SpawnParticles(r, c, Color.white);
                    // }
                    totalPointMult += board.ClearTile(c, r);
                }
            }
            board.AllTileGravity();

            // Because this may cause a tile to fall outside of a blob, unglow un blob tiles
            board.UnglowNotInBlobs();

            float bombDamageMult = 1.5f;
            board.DealDamage((int)(board.damagePerMana*totalPointMult*bombDamageMult), explosionCenter, partOfChain: false);
        }

        
        public void MakeGoldMine(GameBoard board) {
            Debug.Log("gold mine piece creation");

            effect = Battler.ActiveAbilityEffect.GoldMine;

            // // (Old) Tile color mirrors the center color of the current piece it is replacing
            // center.SetColor(board.GetPiece().GetCenter().color, board);

            // (New) tile is always multicolor
            center.SetManaColor(ManaColor.Multicolor, board, setVisual: true);
            // make tile semi transparent white color & multicolor mana icon
            center.visual.SetColor(new Color(1f, 1f, 1f, 0.5f));

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
                board.cosmetics.goldMineObject, 
                center.transform.position + Vector3.forward*2, 
                Quaternion.identity, 
                center.transform
            ); 

            // set material to cycle's corresponding crystal material
            // (nvm crystals dont have colors anymore)        
            // crystal.GetComponent<MeshRenderer>().material = board.cycle.crystalMaterials[center.manaColor];
        }

        public void MakeZman(GameBoard board) {
            center.SetManaColor(ManaColor.Colorless, board, setVisual: false);
            center.visual.SetSprite(board.cosmetics.miniZmanSprite);
            center.MakeObscuresColor();
            center.MakeFragile();
            center.pointMultiplier -= 1.0f;
        }
    }
}