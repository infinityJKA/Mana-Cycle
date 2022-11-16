using UnityEngine;

public class Piece : MonoBehaviour
{
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
    public Orientation orientation = Orientation.up;

    // Theoretically, these should stay the same as this piece's gameobject position, but I don't trust floats.
    // X position of this piece on the grid. Start out at x=3.
    private int x = 3;
    // Y position of this piece on the grid. Start out at y=0;
    private int y = 0;

    // Orientation is the way that the "top" tile is facing
    public enum Orientation
    {
        up,
        left,
        down,
        right
    }

    // Randomize this piece's tiles' colors.
    public void Randomize()
    {
        center.Randomize();
        top.Randomize();
        right.Randomize();
    }

    // Translate this piece by the given X and Y.
    public void Move(int x, int y) {
        this.x += x;
        this.y += y;
        transform.position = new Vector3(x,y,0); // update visually
    }
    public void Move(Vector2Int offset) {
        Move(offset.x, offset.y);
    }

    // quirky enum addition and subtraction (ordinal values)
    // Rotate this piece to the right about the center.
    public void RotateRight()
    {
        if (orientation == Orientation.up) {
            orientation = Orientation.left;
        } else {
            orientation -= 1;
        }
    }

    // Rotate this piece to the left about the center.
    public void RotateLeft()
    {
        if (orientation == Orientation.left) {
            orientation = Orientation.up;
        } else {
            orientation += 1;
        }
    }

    // Update the actual displayed rotation.
    public void UpdateDisplayedRotation()
    {
        transform.eulerAngles = new Vector3(0, 0, ((int)orientation) * 90);
    }

    // Iteration of all coordinates this piece currently occupies.
    public System.Collections.Generic.IEnumerator<Vector2Int> GetEnumerator()
    {
        // Center
        yield return new Vector2Int(x, y);

        // Only return positions off the center if they are taken up by the current orientation
        // Tile on top
        if (orientation == Orientation.up || orientation == Orientation.left) 
            yield return new Vector2Int(x, y+1);

        // Tile to right
        if (orientation == Orientation.right || orientation == Orientation.up) 
            yield return new Vector2Int(x+1, y);

        // Tile on bottom
        if (orientation == Orientation.down || orientation == Orientation.right) 
            yield return new Vector2Int(x, y-1);

        // Tile to left
        if (orientation == Orientation.left || orientation == Orientation.down) 
            yield return new Vector2Int(x-1, y);
    }
}