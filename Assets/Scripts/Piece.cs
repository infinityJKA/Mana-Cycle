using System;

public class Piece
{
    public readonly static Random random = new Random();

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
    private int x;
    // Y position of this piece on the grid
    private int y;


    /// <summary> Create a new piece, facing UP, with randomized colors. </summary>
    public Piece()
    {
        // Randomize all colors of this piece.
        // Add 1 so that NONE at the start of the ManaColor enum is skipped.
        center = random.Next(5) + 1;
        top = random.Next(5) + 1;
        right = random.Next(5) + 1;
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

    // Translate this tile by the given amount.
    public void Move(int x, int y)
    {
        this.x += x;
        this.y += y;
    }
}