using UnityEngine;
using System.Collections.Generic;

public class Piece : MonoBehaviour
{
    // Theoretically, these should stay the same as this piece's gameobject position, but I don't trust floats.
    // Column position of this piece on the grid.
    [SerializeField] private int col = 0;
    // Row position of this piece on the grid.
    [SerializeField] private int row = 0;

    // Think of each piece as an L on your left hand. Top is pointer finger and right is thumb.
    // Tile in center
    [SerializeField] private Tile center; 
    // Tile on top (unrotated)
    [SerializeField] private Tile top;
    // Tile on right (unrotated)
    [SerializeField] private Tile right;
    
    // Rotation center - holds all the tile objects. Centered on tile for correct visual rotation.
    [SerializeField] private Transform rotationCenter;

    // This piece's rotation, direction that the top tile is facing. Start out facing up.
    [SerializeField] private Orientation orientation = Orientation.up;

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

    private Vector3 UndoOrientedDirection()
    {
        switch(orientation)
        {
            case Orientation.up: return Vector3.up;
            case Orientation.left: return Vector3.right;
            case Orientation.down: return Vector3.down;
            case Orientation.right: return Vector3.left;
            default: return Vector3.zero;
        }
    }

    // Randomize the color of the tiles of this piece.
    public void Randomize(GameBoard board)
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
    }

    private ManaColor RandomColor()
    {
        return (ManaColor)Random.Range(0, ManaCycle.cycleUniqueColors);
    }

    private ManaColor ColorWeightedRandom(GameBoard board)
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
        if (this != null) transform.localPosition = new Vector3(this.col - 4, -this.row + 7, 0);
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
    public void UpdateOrientation()
    {
        rotationCenter.rotation = Quaternion.LookRotation(Vector3.forward, OrientedDirection());

        // make the inner tiles face opposite rotation, so animation stays correct
        var opposite = UndoOrientedDirection();
        center.transform.rotation = Quaternion.LookRotation(Vector3.forward, opposite);
        top.transform.rotation = Quaternion.LookRotation(Vector3.forward, opposite);
        right.transform.rotation = Quaternion.LookRotation(Vector3.forward, opposite);
    }

    // Iteration of all coordinates this piece currently occupies. Returns Vector2Ints of (col, row).
    public IEnumerator<Vector2Int> GetEnumerator()
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
    public void PlaceTilesOnBoard(ref Tile[,] board)
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