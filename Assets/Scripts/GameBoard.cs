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
    // Input prefab containing a script component
    [SerializeField] private GameObject inputObject;
    private InputScript inputScript;

    // Cache the ManaCycle in this scene. (on start)
    private ManaCycle cycle;

    // Cycle pointer game object that belongs to this board
    private GameObject pointer;
    // This board's current position in the cycle. starts at 0
    public int cyclePosition;

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

    // Amount of times the player has cleared the cycle.
    private int currentCycle = 0;

    // Start is called before the first frame update
    void Start()
    {
        // script containing keycodes for controls
        inputScript = inputObject.GetComponent<InputScript>();
    }

    // Initialize with a passed cycle. Taken out of start because it relies on ManaCycle's start method
    public void InitializeCycle(ManaCycle cycle)
    {
        this.cycle = cycle;

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
            // rotate left
            if (Input.GetKeyDown(inputScript.RotateLeft))
            {
                piece.RotateLeft();
                if(!ValidPlacement()){
                    piece.RotateRight();
                }
            }

            // rotate right
            if (Input.GetKeyDown(inputScript.RotateRight))
            {
                piece.RotateRight();
                if(!ValidPlacement()){
                    piece.RotateLeft();
                }
            }

            // move piece left
            if (Input.GetKeyDown(inputScript.Left))
            {
                MovePiece(-1, 0);
            }

            // move piece right
            if (Input.GetKeyDown(inputScript.Right))
            {
                MovePiece(1, 0);
            }

            // Spellcast
            if (Input.GetKeyDown(inputScript.Cast))
            {
                // get current mana color from cycle, and clear that color
                ManaColor clearColor = cycle.GetCycle()[cyclePosition];
                Spellcast(clearColor, 1, 1);
            }

            // Get the time that has passed since the previous piece fall.
            // If it is greater than fall time (or fallTime/10 if holding down),
            // move the piece one down.
            if(Time.time - previousFallTime > (Input.GetKey(inputScript.Down) ? fallTime/10 : fallTime)){
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
        Transform manaColor = cycle.transform.GetChild(cyclePosition);
        pointer.transform.SetParent(manaColor, false);
        // Move left or right based on if this is the player or not
        if (playerControlled) {
            pointer.transform.localPosition = new Vector3(-100, 0, 0);
        } else {
            pointer.transform.localPosition = new Vector3(100, 0, 0);
        }

    }

    // Create a new piece and spawn it at the top of the board. Replaces the current piece field.
    public void SpawnPiece()
    {
        // Creates a new piece at the spawn location.
        // The new tiles' position Vector2 will determine how it is rendered.
        piece = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity, transform);

        // Randomize the piece's tiles' colors
        piece.Randomize(this);

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

        Destroy(piece);

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
    }

    // Clear the tile at the given index, destroying the Tile gameObject.
    public void ClearTile(int col, int row)
    {
        Destroy(board[row, col].gameObject);
        board[row, col] = null;
    }

    public struct Blob
    {
        public ManaColor color;
        public List<Vector2Int> tiles;
    }

    // Deal damage to the other player(s)
    public void DealDamage(float damage)
    {
        // TODO: implement damage dealing
    }

    //
    private static bool[,] tilesInBlobs;

    // Check the board for blobs of 4 or more of the given color, and clear them from the board, earning points/dealing damage.
    public void Spellcast(ManaColor color, int chain, int cascade)
    {
        // Save matrix of all tiles currently in one of the blobs
        tilesInBlobs = new bool[height, width];

        // List of all blobs
        List<Blob> blobs = new List<Blob>();
        int minBlobSize = 3;

        // Loop over rows (top to bottom)
        for (int r = 0; r < height; r++)
        {
            // Loop over columns (left to right)
            for (int c = 0; c < width; c++)
            {
                // Check if indexed tile exists and is correct color, and is not in a blob
                if (board[r, c] != null && board[r, c].GetManaColor() == color && !tilesInBlobs[r, c])
                {
                    Blob blob = CheckBlob(c, r, color);
                    if (blob.tiles.Count >= minBlobSize) blobs.Add(blob);
                }
            }
        }

        // If there were no blobs, do not deal damage, and do not move forward in cycle
        if (blobs.Count == 0) return;

        int manaCleared = TotalMana(blobs);
        if (manaCleared > 0) {
            // Deal damage for the amount of mana cleared.
            float damage = (manaCleared * 10) * chain * cascade * (1 + 0.3f*currentCycle);
            DealDamage(damage);

            // Clear all blob-contained tiles from the board.
            foreach (Blob blob in blobs) {
                foreach (Vector2Int pos in blob.tiles) {
                    ClearTile(pos.x, pos.y);
                }
            }

            // Do gravity everywhere
            AllTileGravity();

            // Clear cascaded blobs (add one to cascade)
            Spellcast(color, chain, cascade+1);
        }
        
        // move to next in cycle position
        cyclePosition += 1;
        if (cyclePosition >= ManaCycle.cycleLength) cyclePosition = 0;
        PointerReposition();
    }

    // Returns the total amount of mana in this set of blobs.
    public int TotalMana(List<Blob> blobs)
    {
        int mana = 0;
        foreach (Blob blob in blobs)
        {
            mana += blob.tiles.Count;
        }
        return mana;
    }

    public Blob CheckBlob(int c, int r, ManaColor color)
    {
        Blob blob = new Blob();
        blob.color = color;
        blob.tiles = new List<Vector2Int>();

        ExpandBlob(ref blob, c, r, color, 0);

        return blob;
    }

    public void ExpandBlob(ref Blob blob, int c, int r, ManaColor color, int recurseAmount)
    {
        if (recurseAmount > 100) {
            Debug.LogError("MAX RECURSION REACHED!");
        };

        // Don't add to blob if the tile is in an invalid position
        if (c < 0 || c >= width || r < 0 || r >= height) return;

        // Don't add to blob if already in this blob or another blob
        if (tilesInBlobs[r, c]) return;

        // Don't add if there is not a tile here
        if (board[r, c] == null) return;

        // Don't add if the tile is the incorrect color
        if (board[r, c].GetManaColor() != color) return;

        // Add the tile to the blob and fill in its spot on the tilesInBlobs matrix
        Debug.Log(c + ", " + r + ", " + blob.tiles.Count + ", " + recurseAmount);
        blob.tiles.Add(new Vector2Int(c, r));
        tilesInBlobs[r, c] = true;

        // Expand out the current blob on all sides, checking for the same colored tile to add to this blob
        ExpandBlob(ref blob, c-1, r, color, recurseAmount+1);
        ExpandBlob(ref blob, c+1, r, color, recurseAmount+1);
        ExpandBlob(ref blob, c, r-1, color, recurseAmount+1);
        ExpandBlob(ref blob, c, r+1, color, recurseAmount+1);
    }

    // Check the tile at the given index for gravity,
    // pulling it down to the next available empty tile.
    // Returns true if the fell at all.
    public bool TileGravity(int c, int r)
    {
        // If there isn't a tile here, it can't fall, because it isnt a tile...
        if (board[r, c] == null) return false;

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
