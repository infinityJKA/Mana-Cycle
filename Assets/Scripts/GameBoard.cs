using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    // True if the player controls this board.
    [SerializeField] private bool playerControlled;

    // Piece prefab, containing an object with Tile gameobject children
    [SerializeField] private Piece piecePrefab;
    // Prefab for cycle pointers
    [SerializeField] private GameObject pointerPrefab;
    
    // Cache the ManaCycle in this scene. (on start)
    private ManaCycle cycle;

    // Cycle pointer game object that belongs to this board
    private GameObject pointer;
    // This board's current position in the cycle. starts at 0
    private int cyclePosition;

    // Dimensions of the board
    public static readonly int width = 8;
    public static readonly int height = 14;

    // The last time that the current piece fell down a tile.
    private float previousFallTime;
    [SerializeField] private float fallTime = 0.8f;

    // Board containing all tiles that have been placed and their colors. NONE is an empty space (from ManaColor enum).
    private Tile[,] board;
    // Piece that is currently being dropped.
    private Piece piece;

    // Start is called before the first frame update
    void Start()
    {
        cycle = GameObject.Find("Cycle").GetComponent<ManaCycle>();

        pointer = Instantiate(pointerPrefab, Vector3.zero, Quaternion.identity);
        PointerReposition();

        board = new Tile[height, width];
        if (playerControlled) 
        {
            SpawnPiece();
        }   
    }

    // Update is called once per frame
    void Update()
    {
       if (playerControlled)
       {
        // Z - rotate left
        if(Input.GetKeyDown(KeyCode.Z)){
            piece.RotateLeft();
            if(!ValidPlacement()){
                piece.RotateRight();
            }
        }

        // X - rotate right
        if(Input.GetKeyDown(KeyCode.X)){
            piece.RotateRight();
            if(!ValidPlacement()){
                piece.RotateLeft();
            }
        }

        // Left Arrow - move piece left
        if(Input.GetKeyDown(KeyCode.LeftArrow)){
            MovePiece(-1, 0);
        }

        // Right Arrow - move piece right
        if(Input.GetKeyDown(KeyCode.RightArrow)){
            MovePiece(1, 0);
        }

        // Get the time that has passed since the previous piece fall.
        // If it is greater than fall time (or fallTime/10 if holding down),
        // move the piece one down.
        if(Time.time - previousFallTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime/10 : fallTime)){
            // Try to move the piece down. If it can't be moved down,
            if (!MovePiece(0, 1))
            {
                // Place the piece
                PlacePiece();
                // Spawn a new piece
                SpawnPiece();
            }         
            // reset fall time
            previousFallTime = Time.time;   
        }
       }
    }

    // Update the pointer's cycle position.
    private void PointerReposition()
    {
        // Get the position of the ManColor the pointer is supposed to be on
        pointer.transform.position = cycle.transform.GetChild(cyclePosition).transform.position;
        // Move left or right based on if this is the player or not
        if (playerControlled) {
            pointer.transform.position += new Vector3(-100, 0, 0);
        } else {
            if (playerControlled) {
            pointer.transform.position += new Vector3(100, 0, 0);
        }
        }

    }

    // Create a new piece and spawn it at the top of the board. Replaces the current piece field.
    public void SpawnPiece()
    {
        // Creates a new piece at the spawn location.
        // The new tiles' position Vector2 will determine how it is rendered.
        piece = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity, transform);

        // Randomize the piece's tiles' colors
        piece.Randomize();

        // Move the tile to the spawn location
        piece.MoveTo(3,1);
    }

    // Move the current piece by this amount. 
    // Return true if the piece is not blocked from moving to the new location.
    public bool MovePiece(int col, int row)
    {
        piece.Move(col, row);
        // Check if the piece now overlaps any grid tiles, if so, move the tile back and return false
        if (!ValidPlacement()) {
            piece.Move(-col, -row);
            return false;
        }
        return true;
    }

    // Return true if the current piece is in a valid position on the grid (not overlapping any tiles), false if not.
    public bool ValidPlacement()
    {
        foreach (Vector2Int tile in piece)
        {
            // Return false if the tile is in an invalid position
            // || tile.y < 0 - tiles can be above the ceiling, but if placed there, player dies.
            if (tile.x < 0 || tile.x >= width || tile.y >= height) return false;

            // Return false here if the grid position's value is not null, so another tile is there
            if (board[tile.y, tile.x] != null) return false;
        }
        // No tiles overlapped, return true
        return true;
    }

    // Place a piece on the grid, moving its Tiles into the board array and removing the Piece.
    public void PlacePiece()
    {
        piece.PlaceTilesOnBoard(ref board);

        // Move the displayed tiles into the board parent
        piece.GetCenter().transform.SetParent(transform, true);
        piece.GetTop().transform.SetParent(transform, true);
        piece.GetRight().transform.SetParent(transform, true);

        bool tileFell = true;
        // Keep looping until none of the piece's tiles fall
        while (tileFell) {
            tileFell = false;
            
            // Affect all placed tiles with gravity.
            foreach (Vector2Int tile in piece)
            {
                // Only do gravity if this tile is still here and hasn't fallen to gravity yet
                if (board[tile.y, tile.x] != null)
                {
                    // If a tile fell, set tileFell to true and the loop will go again after this
                    if (TileGravity(tile.x, tile.y))
                    {
                        tileFell = true;
                    }
                }
            }
        }

        Debug.Log("Placed piece at " + piece.GetCol() + ", " + piece.GetRow());
    }

    // Check the board for lines of the given color and clear them from the board, earning points/dealing damage.
    public void CheckForLines(ManaColor color)
    {
        // Check for color lines, and add them to a list of Vector3Ints.
        // Coordinates represent (row, column, length).
        // Length of the current line is stored here.
        int currentLength = 0;

        // ---- Horizontal lines ----
        List<Vector3Int> horizontalLines = new List<Vector3Int>(); // (facing right)
        // Loop over rows (top to bottom)
        for (int r = 0; r < height; r++)
        {
            // Loop over columns (left to right)
            for (int c = 0; c < width; c++)
            {
                // Check if indexed tile is correct color
                if (board[r, c].GetManaColor() == color)
                {
                    // If so, increase line length
                    currentLength++;
                } else {
                    // if not, line has ended
                    _AddLine(r, c-currentLength, ref horizontalLines);
                }
            }
            // End current line tracking here at the end of the board if one is still going
            _AddLine(r, width-currentLength, ref horizontalLines);
        }

        // |||| Vertical lines ||||
        List<Vector3Int> verticalLines = new List<Vector3Int>(); // (facing down)
        // Loop over columns (left to right)
        for (int c = 0; c < width; c++)
        {
            // Loop over rows (top to bottom)
            for (int r = 0; r < height; r++)
            {
                // Check if indexed tile is correct color
                if (CheckColor(r, c, color))
                {
                    // If so, increase line length
                    currentLength++;
                } else {
                    // if not, line has ended
                    _AddLine(r-currentLength, c, ref verticalLines);
                }
            }
            // End current line tracking here at the end of the board if one is still going
            _AddLine(height-currentLength, c, ref verticalLines);
        }

        // (Local to checkLines) Checks if line is 4 or greater, if so, add the line. 
        // Resets currentLength.
        void _AddLine(int r, int c, ref List<Vector3Int> linesList)
        {
            // Check if line is 4 or greater, if so, add the line
            if (currentLength >= 4)
            {
                linesList.Add(new Vector3Int(r, c, currentLength));
            }
            currentLength = 0;
        }

        // Okay now that we finally have all the lines' positions and lengths, clear them all from the board.
        // TODO: implement scoring based on the length of the lines.
        // ---- horizontal lines ----
        foreach (Vector3Int line in horizontalLines)
        {
            // Z is the length of the line
            for (int c = 0; c < line.z; c++)
            {
                board[line.y, line.x + c] = null;
            }
        }

        // ---- vertical lines ----
        foreach (Vector3Int line in verticalLines)
        {
            // Z is the length of the line
            for (int r = 0; r < line.z; r++)
            {
                board[line.y + r, line.x] = null;
            }
        }

        // Now finally, shift all tiles down.
        AllTileGravity();

        // If any lines were cleared, some tiles may have fallen to create a new line,
        // so repeat recursively until there are no more row/line clears.
        if (horizontalLines.Count > 0 || verticalLines.Count > 0)
        {
            CheckForLines(color);
        }
    }

    // Check the tile at the given index for gravity,
    // pulling it down to the next available empty tile.
    // Returns true if the fell at all.
    public bool TileGravity(int c, int r)
    {
        // For each tile, check down until there is no longer an empty tile
        for (int rFall = r+1; rFall <= height; rFall++)
        {
            // Once a non-empty is found, or reached the bottom move the tile to right above it
            if (rFall == height || board[rFall, c] != null)
            {
                // skip if tile is in same location
                if (rFall-1 != r) {
                    board[rFall-1, c] = board[r, c];
                    // I am subtracting half of width and height again here, because it only works tht way,
                    // i don't know enough about transforms to know why. bandaid solution moment.
                    board[rFall-1, c].transform.localPosition = new Vector3(c - 3.5f, -rFall + 1 + 6.5f, 0);
                    board[r, c] = null;
                    return true;
                }
                break;
            }
        }
        return false;
    }

    // Affect all tiles with gravity.
    public void AllTileGravity()
    {
        // Loop over columns (left to right)
        for (int c = 0; c < width; c++)
        {
            // Loop over rows (BOTTOM to top)
            // Skip bottom tiles, as those cannot fall
            for (int r = height-2; r >= 0; r--)
            {
                TileGravity(c, r);
            }
        }
    }

    // Check if the tile at the index is the given color, if there is one there.
    public bool CheckColor(int r, int c, ManaColor color)
    {
        // return false if there is no tile at the index
        if (board[r,c] == null) return false;
        // if there is a tile, return true if it is the given color.
        return board[r,c].GetManaColor() == color;
    }
}
