using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBoard : MonoBehaviour
{
    // The battler selected for this board. Each one has different effects.
    [SerializeField] private Battler battler;
    // True if the player controls this board.
    [SerializeField] private bool playerControlled;
    // 0 for left side, 1 for right side
    [SerializeField] private int playerSide;

    // Prefab for cycle pointers
    [SerializeField] private GameObject pointerPrefab;
    // Input mapping for this board
    [SerializeField] private InputScript inputScript;

    // The board of the enemy of the player/enemy of this board
    [SerializeField] private GameBoard enemyBoard;
    // HP Bar game object on this board
    [SerializeField] private HpBar hpBar;

    // Stores the piece preview for this board
    [SerializeField] private PiecePreview piecePreview;
    // Stores the board's cycle level indicator
    [SerializeField] private CycleLevel cycleLevelDisplay;
    // Stores the image for the portrait
    [SerializeField] private Image portrait;

    // Current fall delay for pieces.
    [SerializeField] private float fallTime = 0.8f;



    // Starting HP of this character.
    public int maxHp { get; private set; }
    // Amount of HP this player has remaining.
    public int hp { get; private set; }

    // Stores the ManaCycle in this scene. (on start)
    public ManaCycle cycle { get; private set; }

    // Cycle pointer game object that belongs to this board
    private GameObject pointer;
    // This board's current position in the cycle. starts at 0
    public int cyclePosition { get; private set; }

    // Dimensions of the board
    public static readonly int width = 8;
    public static readonly int height = 14;

    // The last time that the current piece fell down a tile.
    private float previousFallTime;
    // If this board is currently spellcasting (chaining).
    private bool casting = false;

    // Board containing all tiles that have been placed and their colors. NONE is an empty space (from ManaColor enum).
    private Tile[,] board;
    // Piece that is currently being dropped.
    private Piece piece;

    // Amount of times the player has cleared the cycle. Used in daamge formula
    private int cycleLevel = 0;

    // Cached pause menu, so this board can pause the game
    private PauseMenu pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        // (Later, this may depend on the character/mode)
        maxHp = 2000;
        hp = maxHp;

        // Cache stuff
        pauseMenu = GameObject.FindObjectOfType<PauseMenu>();

        // Setup battler
        portrait.sprite = battler.sprite;
    }

    // Initialize with a passed cycle. Taken out of start because it relies on ManaCycle's start method
    public void InitializeCycle(ManaCycle cycle)
    {
        this.cycle = cycle;

        pointer = Instantiate(pointerPrefab, Vector3.zero, Quaternion.identity);
        PointerReposition();

        piecePreview.Setup(this);
        hpBar.Setup(this);

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
            if (Input.GetKeyDown(inputScript.Pause))
            {
                pauseMenu.TogglePause();
            }

            // control the pause menu if paused
            if (pauseMenu.paused)
            {
                if (Input.GetKeyDown(inputScript.Up)) {
                    pauseMenu.MoveCursor(1);
                } else if (Input.GetKeyDown(inputScript.Down)) {
                    pauseMenu.MoveCursor(-1);
                }
            } 
            
            // If not paused, do piece movements
            else {
                // rotate left
                if (Input.GetKeyDown(inputScript.RotateLeft))
                {
                    piece.RotateLeft();
                    if(!ValidPlacement()){
                        // try nudging left, then right, then up. If none work, undo the rotation
                        if (!MovePiece(-1, 0) && !MovePiece(1, 0) && !MovePiece(0, -1)) piece.RotateRight();
                    }
                }

                // rotate right
                if (Input.GetKeyDown(inputScript.RotateRight))
                {
                    piece.RotateRight();
                    if(!ValidPlacement()){
                        // try nudging right, then left, then up. If none work, undo the rotation
                        if (!MovePiece(1, 0) && !MovePiece(-1, 0) && !MovePiece(0, -1)) piece.RotateLeft();
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
                    // start at chain of 1
                    Spellcast(1);
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
                        // Move self damage cycle
                        DamageCycle();
                    }         
                    // reset fall time
                    previousFallTime = Time.time;   
                }
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
        if (playerSide == 0) {
            pointer.transform.localPosition = new Vector3(-100, 0, 0);
        } else {
            pointer.transform.localPosition = new Vector3(100, 0, 0);
        }

    }

    // Create a new piece and spawn it at the top of the board. Replaces the current piece field.
    public void SpawnPiece()
    {
        piece = piecePreview.SpawnNextPiece();
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

        // Destroy the piece containing the tiles, leaving only the tiles that were just taken out of the piece
        Destroy(piece);

        bool tileFell = true;
        // Keep looping until none of the piece's tiles fall
        // (No other tiles need to be checked as tiles underneath them won't move, only tiles above)
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

    // Deal damage to the other player(s(?))
    public void DealDamage(int damage)
    {
        damage = hpBar.CounterIncoming(damage);
        enemyBoard.EnqueueDamage(damage);
        hpBar.Refresh();
    }

    // Enqueues damage to this board.
    // Called from another board's DealDamage() method.
    public void EnqueueDamage(int damage)
    {
        hpBar.DamageQueue[0].AddDamage(damage);
        hpBar.Refresh();
    }

    // Moves incoming damage and take damage if at end
    public void DamageCycle()
    {
        // Deal damage, if any
        hp -= hpBar.DamageQueue[5].dmg;
        hpBar.AdvanceDamageQueue();
        hpBar.Refresh();
    }


    // Temporary, Only used for finding blobs within a single search, not used outside of search
    private static bool[,] tilesInBlobs;

    struct Blob
    {
        public ManaColor color;
        public List<Vector2Int> tiles;
    }

    private static int minBlobSize = 3;

    // Check the board for blobs of 4 or more of the given color, and clear them from the board, earning points/dealing damage.
    private void Spellcast(int chain)
    {
        // Don't start a spellcast if already spellcasting
        if (casting) {Debug.Log("cast failed"); return;}
        Debug.Log("cast start");
        // Save matrix of all tiles currently in one of the blobs
        tilesInBlobs = new bool[height, width];

        // List of all blobs
        ManaColor color = cycle.GetCycle()[cyclePosition];
        List<Blob> blobs = ClearManaOfColor(color);

        // If there were no blobs, do not deal damage, and do not move forward in cycle, end spellcast
        if (blobs.Count == 0) {
            Debug.Log("end cast");
            casting = false;
            return;
        };
        int manaCleared = TotalMana(blobs);

        // Keep clearing while mana cleared for current color is greater than 0
        // Begin spellcast
        int cascade = 0;
        casting = true;
        StartCoroutine(ClearCascadeWithDelay());
        IEnumerator ClearCascadeWithDelay()
        {
            while (manaCleared > 0) {
                // If this is cascading off the same color more than once, short delay between
                yield return new WaitForSeconds(0.5f);

                cascade += 1;

                // DMG per mana, starts at 10, increases by 3 per cycle level
                int damagePerMana = 10 + 3*cycleLevel;
                // Deal damage for the amount of mana cleared.
                // DMG is scaled by chain and cascade.
                int damage = (int)( (manaCleared * damagePerMana) * chain * cascade );

                // Send the damage over. Will subtract from incoming damage first.
                DealDamage(damage);

                // Clear all blob-contained tiles from the board.
                foreach (Blob blob in blobs) {
                    foreach (Vector2Int pos in blob.tiles) {
                        ClearTile(pos.x, pos.y);
                    }
                }

                // Do gravity everywhere
                AllTileGravity();

                // Check for cascaded blobs
                List<Blob> cascadedBlobs = ClearManaOfColor(color);
                manaCleared = TotalMana(cascadedBlobs);
            }

             // move to next in cycle position
            cyclePosition += 1;
            if (cyclePosition >= ManaCycle.cycleLength) {
                cyclePosition = 0;
                cycleLevel++;
                cycleLevelDisplay.Set(cycleLevel);
            }
            PointerReposition();

            // Spellcast for the new next color to check for chain
            StartCoroutine(SpellcastAfterDelay());
            IEnumerator SpellcastAfterDelay()
            {
                yield return new WaitForSeconds(0.75f);
                Spellcast(chain+1);
            }
        }
    }

    // Clears the given color from the board.
    // Returns a list of all blobs of mana that were cleared.
    private List<Blob> ClearManaOfColor(ManaColor color)
    {
        List<Blob> blobs = new List<Blob>();

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

        return blobs;
    }

    // Returns the total amount of mana in this set of blobs.
    int TotalMana(List<Blob> blobs)
    {
        int mana = 0;
        foreach (Blob blob in blobs)
        {
            mana += blob.tiles.Count;
        }
        return mana;
    }

    Blob CheckBlob(int c, int r, ManaColor color)
    {
        Blob blob = new Blob();
        blob.color = color;
        blob.tiles = new List<Vector2Int>();

        ExpandBlob(ref blob, c, r, color);

        return blob;
    }

    void ExpandBlob(ref Blob blob, int c, int r, ManaColor color)
    {
        // Don't add to blob if the tile is in an invalid position
        if (c < 0 || c >= width || r < 0 || r >= height) return;

        // Don't add to blob if already in this blob or another blob
        if (tilesInBlobs[r, c]) return;

        // Don't add if there is not a tile here
        if (board[r, c] == null) return;

        // Don't add if the tile is the incorrect color
        if (board[r, c].GetManaColor() != color) return;

        // Add the tile to the blob and fill in its spot on the tilesInBlobs matrix
        blob.tiles.Add(new Vector2Int(c, r));
        tilesInBlobs[r, c] = true;

        // Expand out the current blob on all sides, checking for the same colored tile to add to this blob
        ExpandBlob(ref blob, c-1, r, color);
        ExpandBlob(ref blob, c+1, r, color);
        ExpandBlob(ref blob, c, r-1, color);
        ExpandBlob(ref blob, c, r+1, color);
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

    public ManaColor CurrentColor()
    {
        return cycle.GetColor(cyclePosition);
    }

    public PieceRng GetPieceRng()
    {
        return battler.pieceRng;
    }
}
