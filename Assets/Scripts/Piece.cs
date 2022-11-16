using UnityEngine;

public class Piece
{
    // Think of each piece as an L on your left hand. Top is pointer finger and right is thumb.
    // Tile in center
    public readonly int center; 
    // Tile on top (unrotated)
    public readonly int top; 
    // Tile on right (unrotated)
    public readonly int right;
    // This piece's rotation, direction that the top tile is facing
    public readonly Orientation orientation;
    
    // X position of this piece on the grid
    private int x { get; set; }
    // Y position of this piece on the grid
    private int y { get; set; }


    /// <summary> Create a new piece, facing UP, with randomized colors. </summary>
    public Piece()
    {
        // Randomize all colors of this piece.
        // Skip first ManaColor of NONE
        center = Random.Range(1,6);
        top = Random.Range(1,6);
        right = Random.Range(1,6);
        orientation = Orientation.UP;

        // Begin with this tile at the top of the grid.
        x = 3;
        y = 0;
    }

    // Orientation is the way that the "top" tile is facing
    public enum Orientation
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    // Translate this piece by the given X and Y.
    public void Move(int x, int y) {
        this.x += x;
        this.y += y;
    }
    public void Move(Vector2Int offset) {
        Move(offset.x, offset.y);
    }

    // Iteration of all coordinates this piece currently occupies.
    public System.Collections.Generic.IEnumerator<Vector2Int> GetEnumerator()
    {
        // Center
        yield return new Vector2Int(x, y);

        // Only return positions off the center if they are taken up by the current orientation
        // Tile on top
        if (orientation == Orientation.UP || orientation == Orientation.LEFT) 
            yield return new Vector2Int(x, y+1);

        // Tile to right
        if (orientation == Orientation.RIGHT || orientation == Orientation.UP) 
            yield return new Vector2Int(x+1, y);

        // Tile on bottom
        if (orientation == Orientation.DOWN || orientation == Orientation.RIGHT) 
            yield return new Vector2Int(x, y-1);

        // Tile to left
        if (orientation == Orientation.LEFT || orientation == Orientation.DOWN) 
            yield return new Vector2Int(x-1, y);
    }
}