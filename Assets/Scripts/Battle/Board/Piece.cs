using UnityEngine;
using System.Collections.Generic;

using Battle.Cycle;

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
        [SerializeField] protected Tile center; 
        // Tile on top (unrotated)
        [SerializeField] protected Tile top;
        // Tile on right (unrotated)
        [SerializeField] protected Tile right;
        
        // Rotation center - holds all the tile objects. Centered on tile for correct visual rotation.
        [SerializeField] protected Transform rotationCenter;

        // This piece's rotation, direction that the top tile is facing. Start out facing up.
        [SerializeField] protected Orientation orientation = Orientation.up;

        // TODO: Move these out of the piece class... otherwise, each piece uses a different bag
        // needed to make it static for now, so both players will use the same bag until this is fixed
        private static int bagPullAmount = -1;
        private static List<ManaColor> currentBag;

        // Orientation is the way that the "top" tile is facing
        public enum Orientation
        {
            up,
            left,
            down,
            right
        }

        // void Update()
        // {
            // Vector3 targetDirection = OrientedDirection();
            // Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
            // rotationCenter.transform.rotation = Quaternion.RotateTowards(rotationCenter.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        // }

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

        // private Vector3 UndoOrientedDirection()
        // {
        //     switch(orientation)
        //     {
        //         case Orientation.up: return Vector3.up;
        //         case Orientation.left: return Vector3.right;
        //         case Orientation.down: return Vector3.down;
        //         case Orientation.right: return Vector3.left;
        //         default: return Vector3.zero;
        //     }
        // }

        // Randomize the color of the tiles of this piece.
        public virtual void Randomize(GameBoard board)
        {
            PieceRng rng = board.GetPieceRng();

            if (rng == PieceRng.CurrentColorWeighted)
            {
                // Randomly choose color from color enum length
                // Color has a 15% chance to boe current color, and 85% chance to be random color (including current).
                // color = (Random.value < 0.2) ? board.CurrentColor() : (ManaColor)Random.Range(0,5); <-- will be infinity's rng pattern
                center.SetColor(ColorWeightedRandom(board), board);
                top.SetColor(ColorWeightedRandom(board), board);
                right.SetColor(ColorWeightedRandom(board), board);
            }

            else if (rng == PieceRng.PureRandom)
            {
                // Randomly choose color from color enum length
                // Color has a 15% chance to boe current color, and 85% chance to be random color (including current).
                // color = (Random.value < 0.2) ? board.CurrentColor() : (ManaColor)Random.Range(0,5); <-- will be infinity's rng pattern
                center.SetColor(RandomColor(), board);
                top.SetColor(RandomColor(), board);
                right.SetColor(RandomColor(), board);
            }

            else if (rng == PieceRng.Bag)
            {
                // select color from randomized bag
                // keep track of how many times a color has been pulled for reshuffle
                center.SetColor(pullColor(), board);
                top.SetColor(pullColor(), board);
                right.SetColor(pullColor(), board);

            }
        }

        protected static ManaColor RandomColor()
        {
            return (ManaColor)Random.Range(0, ManaCycle.cycleUniqueColors);
        }

        protected static List<ManaColor> GenerateColorBag()
        {
            // generate the next piece colors with 2x bag, where x is unique cycle colors
            // create the unsorted list with 2 of each color
            List<ManaColor> newBag = new List<ManaColor>();
            for (int i = 0; i < ManaCycle.cycleUniqueColors; i++)
            {
                newBag.Add( (ManaColor) i);
                newBag.Add( (ManaColor) i);
            }
            // Debug.Log(string.Join(",",newBag));

            Utils.Shuffle(newBag);
            return newBag;
        }

        // pull the next color from bag
        protected static ManaColor pullColor()
        {
            // if end of bag (or first pull), reshuffle
            if (bagPullAmount == -1 || bagPullAmount > currentBag.Count )
            {
                currentBag = GenerateColorBag();
                bagPullAmount = 0;
            }
            ManaColor pulledColor = currentBag[bagPullAmount];
            bagPullAmount++;
            return pulledColor;
        }

        protected static ManaColor ColorWeightedRandom(GameBoard board)
        {
            if (Random.value < 0.15)
            {
                return board.CurrentColor();
            } else {
                return RandomColor();
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
            if (this != null) transform.localPosition = new Vector3(this.col - GameBoard.width/2f, -this.row + GameBoard.physicalHeight/2f + GameBoard.height - GameBoard.physicalHeight, 0);
        }
            
        // Rotate this piece to the right about the center.
        public void RotateRight()
        {
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

        // Update the roation of this object's rotation center, after orientation changes.
        public virtual void UpdateOrientation()
        {
            rotationCenter.rotation = Quaternion.LookRotation(Vector3.forward, OrientedDirection());

            // make the inner tiles face opposite rotation, so animation stays correct
            // var opposite = UndoOrientedDirection();
            center.GetComponent<RectTransform>().rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            top.GetComponent<RectTransform>().rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            right.GetComponent<RectTransform>().rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        // Iteration of all coordinates this piece currently occupies. Returns Vector2Ints of (col, row).
        public virtual IEnumerator<Vector2Int> GetEnumerator()
        {
            // Center
            yield return new Vector2Int(col, row);

            // Only return positions off the center if they are taken up by the current orientation
            // Tile on top
            if (orientation == Orientation.up || orientation == Orientation.left) 
                yield return new Vector2Int(col, row-1);

            // Tile to right
            if (orientation == Orientation.right || orientation == Orientation.up) 
                yield return new Vector2Int(col+1, row);

            // Tile on bottom
            if (orientation == Orientation.down || orientation == Orientation.right) 
                yield return new Vector2Int(col, row+1);

            // Tile to left
            if (orientation == Orientation.left || orientation == Orientation.down) 
                yield return new Vector2Int(col-1, row);
        }

        // Place this tile's pieces onto the passed board.
        public virtual void PlaceTilesOnBoard(ref Tile[,] board, Transform pieceBoard)
        {
            // Place center tile
            board[row, col] = center;

            // Place top and right tile based on orientation
            switch (orientation)
            {
                case Orientation.up:
                    board[row-1, col] = top;
                    board[row, col+1] = right;
                    break;
                case Orientation.left:
                    board[row, col-1] = top;
                    board[row-1, col] = right;
                    break;
                case Orientation.down:
                    board[row+1, col] = top;
                    board[row, col-1] = right;
                    break;
                case Orientation.right:
                    board[row, col+1] = top;
                    board[row+1, col] = right;
                    break;
            }

            // Change parent of tiles
            center.transform.SetParent(pieceBoard, true);
            center.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

            top.transform.SetParent(pieceBoard, true);
            top.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

            right.transform.SetParent(pieceBoard, true);
            right.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        /// <summary>
        /// Called when this piece is placed
        /// (used in actie abilities)
        /// </summary>
        public virtual void OnPlace(GameBoard board)
        {  
            // see SinglePiece.cs for implementation
        }

        public virtual void DestroyTiles() {
            Destroy(center.gameObject);
            Destroy(top.gameObject);
            Destroy(right.gameObject);
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

        public Tile GetCenter()
        {
            return center;
        }

        public Tile GetTop()
        {
            return top;
        }

        public Tile GetRight()
        {
            return right;
        }

        public Orientation getRot(){
            return orientation;
        }
    }
}