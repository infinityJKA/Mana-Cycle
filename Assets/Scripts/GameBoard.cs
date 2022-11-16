using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public static readonly int height = 8;
    public static readonly int width = 14;
    public static readonly int tilePixelSize = 25;

    // Board containing all tiles that have been placed and their colors. NONE is an empty space (from ManaColor enum).
    private ManaColor[,] board;
    // Piece that is currently being dropped.
    private Piece piece;

    // Start is called before the first frame update
    void Start()
    {
        board = new ManaColor[width, height];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Create a new piece and spawn it at the top of the board
    public void NewPiece()
    {
        // Creates a new piece at the spawn location with randomized colors (check Piece constructor)
        piece = new Piece();
    }

    // Move the current piece by this amount. 
    // Return true if the piece is not blocked from moving to the new location.
    public bool MovePiece(int x, int y)
    {
        piece.Move(x, y);
        // Check if the piece now overlaps any grid tiles, if so, move the tile back and return false
        if (!ValidPlacement()) {
            piece.Move(-x, -y);
            return false;
        }
        return true;
    }
    public bool MovePiece(Vector2Int offset)
    {
        return MovePiece(offset.x, offset.y);
    }

    // Return true if the current piece is in a valid position on the grid (not overlapping any tiles), false if not.
    public bool ValidPlacement()
    {
        foreach (Vector2Int tile in piece)
        {
            // Return false here if the ManaColor enum value if the iterated tile of the piece is above 0,
            // or in other words, the grid position's value is not NONE, so another tile is there
            if (board[tile.x, tile.y] > 0) return false;
        }
        // No tiles overlapped, return true
        return true;
    }

    // Check the board for lines of the given color.
    public void checkLines(ManaColor color)
    {
        // Check for color lines, and add them to a list of Vector3Ints.
        // Coordinates represent (row, column, length).
        List<Vector3Int> horizontalLines = new List<Vector3Int>(); // (facing right)
        List<Vector3Int> verticalLines = new List<Vector3Int>(); // (facing down)

        // Length of the current line is stored here.
        int currentLength = 0;
        // ---- Horizontal lines ----
        // Loop over rows (top to bottom)
        for (int r = 0; r < height; r++)
        {
            // Loop over columns (left to right)
            for (int c = 0; c < width; c++)
            {
                // Check if indexed tile is correct color
                if (board[r, c] == color)
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
        // Loop over columns (left to right)
        for (int c = 0; c < width; c++)
        {
            // Loop over rows (top to bottom)
            for (int r = 0; r < height; r++)
            {
                // Check if indexed tile is correct color
                if (board[r, c] == color)
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
                board[line.y, line.x + c] = ManaColor.NONE;
            }
        }

        // ---- vertical lines ----
        foreach (Vector3Int line in verticalLines)
        {
            // Z is the length of the line
            for (int r = 0; r < line.z; r++)
            {
                board[line.y + r, line.x] = ManaColor.NONE;
            }
        }

        // Now finally, shift all tiles down.
        TileGravity();

        // If any lines were cleared, some tiles may have fallen to create a new line,
        // so repeat recursively until there are no more row/line clears.
        if (horizontalLines.Count > 0 || verticalLines.Count > 0)
        {
            checkLines(color);
        }
    }

    // Affect all tiles with gravity, pulling them down to the next available empty
    // tile below.
    public void TileGravity()
    {
        // Loop over columns (left to right)
        for (int c = 0; c < width; c++)
        {
            // Loop over rows (BOTTOM to top)
            // Skip bottom tiles, as those cannot fall
            for (int r = height-2; r >= 0; r--)
            {
                // For each tile, check down until there is no longer an empty tile
                for (int rFall = r+1; r < height; r++)
                {
                    // Once a non-empty is found, move the tile to right above it
                    if (board[rFall,c] != ManaColor.NONE)
                    {
                        // (Can be skipped if the tile did not fall at all)
                        if (rFall-1 > r)
                        {
                            board[rFall-1, c] = board[r, c];
                        }
                    }
                }
            }
        }
    }
}
