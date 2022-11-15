public class Piece
{
    // Think of each piece as an L with the left hand.
    // Numbers from 0-4 for each color.
    public readonly int center; // Tile in center
    public readonly int top; // Tile on top when not rotated
    public readonly int right; // Tile on right when not rotated
    public Orientation orientation;

    // Orientation is the way that "top" is facing
    public enum Orientation
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }
}