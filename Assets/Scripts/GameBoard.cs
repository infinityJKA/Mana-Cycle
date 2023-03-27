using System.Net.NetworkInformation;
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

    // Obect where pieces are drawn
    [SerializeField] public GameObject pieceBoard;

    // Input mapping for this board
    [SerializeField] private InputScript inputScript;

    // The board of the enemy of the player/enemy of this board
    [SerializeField] private GameBoard enemyBoard;
    // HP Bar game object on this board
    [SerializeField] private HealthBar hpBar;

    // Stores the piece preview for this board
    [SerializeField] private PiecePreview piecePreview;

    // Cycle pointer game object that belongs to this board
    [SerializeField] private GameObject pointer;

    // Stores the board's cycle level indicator
    [SerializeField] private CycleLevel cycleLevelDisplay;
    // Stores the image for the portrait
    [SerializeField] private Image portrait;

    // Chain popup object
    [SerializeField] private Popup chainPopup;
    // Cascade popup object
    [SerializeField] private Popup cascadePopup;

    // Current fall delay for pieces.
    [SerializeField] private float fallTime = 0.8f;

    [SerializeField] private GameObject winTextObj;
    private TMPro.TextMeshProUGUI winText;

    // Starting HP of this character.
    public int maxHp { get; private set; }
    // Amount of HP this player has remaining.
    public int hp { get; private set; }

    // Stores the ManaCycle in this scene. (on start)
    public ManaCycle cycle { get; private set; }

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

    // If this board is currently dropping pieces (not dead)
    private bool defeated;

    private float fallTimeMult;

    // True for one frame when new piece spawns
    private bool pieceSpawned;

    private BattlerStorage battlerStorage;

    private bool cycleInitialized;
    private bool postGame = false;
    private PostGameMenu winMenu;

    // Start is called before the first frame update
    void Start()
    {
        // (Later, this may depend on the character/mode)
        maxHp = 2000;
        hp = maxHp;

        // Cache stuff
        pauseMenu = GameObject.FindObjectOfType<PauseMenu>();

        winText = winTextObj.GetComponent<TMPro.TextMeshProUGUI>();
        winMenu = GameObject.Find("PostGameMenu").GetComponent<PostGameMenu>();


        // Setup battler, from char selection screen
        battlerStorage = GameObject.Find("SelectedBattlerStorage").GetComponent<BattlerStorage>();
        // if any value in storage is null, it means we loaded straight to ManaCycle without going to CharSelect first. use default serialized values for battlers
        if (!(battlerStorage.GetBattler1() == null))
        {
            if (playerSide == 0)
            {
                battler = battlerStorage.GetBattler1();
                playerControlled = battlerStorage.GetPlayerControlled1();
            }
            else
            {
                battler = battlerStorage.GetBattler2();
                playerControlled = battlerStorage.GetPlayerControlled2();
            }
            portrait.sprite = battler.sprite;
        }
        
    }

    // Initialize with a passed cycle. Taken out of start because it relies on ManaCycle's start method
    public void InitializeCycle(ManaCycle cycle)
    {
        cycleInitialized = true;
        this.cycle = cycle;

        PointerReposition();

        piecePreview.Setup(this);
        hpBar.Setup(this);

        board = new Tile[height, width];
        SpawnPiece();
    }

    // piece movement is all in functions so they can be called by inputScript. this allows easier implementation of AI controls

    public void RotateLeft(){
        piece.RotateLeft();
        if(!ValidPlacement()){
            // try nudging left, then right, then up. If none work, undo the rotation
            if (!MovePiece(-1, 0) && !MovePiece(1, 0) && !MovePiece(0, -1)) piece.RotateRight();
        }
    }

    public void RotateRight(){
        piece.RotateRight();
        if(!ValidPlacement()){
            // try nudging right, then left, then up. If none work, undo the rotation
            if (!MovePiece(1, 0) && !MovePiece(-1, 0) && !MovePiece(0, -1)) piece.RotateLeft();
        }
    }

    public void MoveLeft(){
        MovePiece(-1, 0);
    }

    public void MoveRight(){
        MovePiece(1, 0);
    }

    public void Spellcast(){
        // get current mana color from cycle, and clear that color
        // start at chain of 1
        // canCast is true if a spellcast is currently in process.
        if (!casting) Spellcast(1);
    }

    public bool isPlayerControlled(){
        return this.playerControlled;
    }

    public bool isDefeated(){
        return this.defeated;
    }

    public bool isPieceSpawned(){
        return this.pieceSpawned;
    }

    public Piece getPiece(){
        return this.piece;
    }

    public void setFallTimeMult(float m){
        this.fallTimeMult = m;
    }

    // Update is called once per frame

    void Update()
    {
        // wait for cycle to initialize (after countdown) to run game logic
        if (!cycleInitialized) return;

        // if (!defeated)
        // {
            if (Input.GetKeyDown(inputScript.Pause) && !postGame)
            {
                pauseMenu.TogglePause();
            }

            // control the pause menu if paused
            if (pauseMenu.paused && !postGame)
            {
                if (Input.GetKeyDown(inputScript.Up)) {
                    pauseMenu.MoveCursor(1);
                } else if (Input.GetKeyDown(inputScript.Down)) {
                    pauseMenu.MoveCursor(-1);
                }

                if (Input.GetKeyDown(inputScript.Cast)){
                    pauseMenu.SelectOption();
                }
            }

            // same with post game menu
            else if (postGame)
            {
                if (Input.GetKeyDown(inputScript.Up)) {
                    winMenu.MoveCursor(1);
                } else if (Input.GetKeyDown(inputScript.Down)) {
                    winMenu.MoveCursor(-1);
                }

                if (Input.GetKeyDown(inputScript.Cast)){
                    winMenu.SelectOption();
                }
            }
            
            // If not paused, do piece movements
            else {
                pieceSpawned = false;

                if (playerControlled){
                    if (Input.GetKey(inputScript.Down)){
                        this.fallTimeMult = 0.1f;
                    }
                    else{
                        this.fallTimeMult = 1f;
                    }
                }

                // Get the time that has passed since the previous piece fall.
                // If it is greater than fall time (or fallTime/10 if holding down),
                // move the piece one down.
                if(Time.time - previousFallTime > fallTime*this.fallTimeMult){
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
        // }
    }

    // Update the pointer's cycle position.
    private void PointerReposition()
    {
        // Get the position of the ManColor the pointer is supposed to be on
        Transform manaColor = cycle.transform.GetChild(cyclePosition);

        pointer.transform.position = new Vector3(
            // Move left or right based on if this is the player or not
            manaColor.transform.position.x + ((playerSide == 0) ? -100 : 100),
            manaColor.transform.position.y,
            0
        );
    }

    // Create a new piece and spawn it at the top of the board. Replaces the current piece field.
    public void SpawnPiece()
    {
        pieceSpawned = true;
        piece = piecePreview.SpawnNextPiece();

        // If the piece is already in an invalid position, player has topped out
        if (!ValidPlacement()) {
            hp = 0;
            Defeat();
        }
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
            if (tile.x < 0 || tile.x >= width || tile.y < 0 || tile.y >= height) return false;

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
        piece.GetCenter().transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        piece.GetTop().transform.SetParent(transform, true);
        piece.GetTop().transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        piece.GetRight().transform.SetParent(transform, true);
        piece.GetRight().transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

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

        // If this player is out of HP, run defeat
        if (hp <= 0) Defeat();
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
        // Save blank matrix of all tiles currently in one of the blobs
        tilesInBlobs = new bool[height, width];

        // List of all blobs
        ManaColor color = cycle.GetCycle()[cyclePosition];
        List<Blob> blobs = FindBlobsOfColor(color);

        // If there were no blobs, do not deal damage, and do not move forward in cycle, 
        // end spellcast if active
        if (blobs.Count == 0) {
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
            // Brief delay before clearing current color
            yield return new WaitForSeconds(0.8f);

            // Check for any new blobs that may have formed in those 0.8 seconds
            // (Replaces old list)
            tilesInBlobs = new bool[height, width];
            blobs = FindBlobsOfColor(color);
            manaCleared = TotalMana(blobs);

            while (manaCleared > 0) {
                // If this is cascading off the same color more than once, short delay between
                if (cascade > 0) yield return new WaitForSeconds(0.5f);

                if (chain > 1) chainPopup.Flash(chain.ToString());

                cascade += 1;
                if (cascade > 1) cascadePopup.Flash(cascade.ToString());

                // DMG per mana, starts at 10, increases by 3 per cycle level
                int damagePerMana = 10 + 3*cycleLevel;
                // Deal damage for the amount of mana cleared.
                // DMG is scaled by chain and cascade.
                int damage = (int)( (manaCleared * damagePerMana) * chain * cascade );

                // Send the damage over. Will counter incoming damage first.
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
                tilesInBlobs = new bool[height, width];
                List<Blob> cascadedBlobs = FindBlobsOfColor(color);
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
            Spellcast(chain+1);
        }
    }

    // Returns a list of all blobs of mana that were cleared.
    private List<Blob> FindBlobsOfColor(ManaColor color)
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

    // Returns the total amount of mana in the passed set of blobs.
    int TotalMana(List<Blob> blobs)
    {
        int mana = 0;
        foreach (Blob blob in blobs)
        {
            mana += blob.tiles.Count;
        }
        return mana;
    }

    // Checks for a blob and recursively expands to all connected tiles
    Blob CheckBlob(int c, int r, ManaColor color)
    {
        Blob blob = new Blob();
        blob.color = color;
        blob.tiles = new List<Vector2Int>();

        ExpandBlob(ref blob, c, r, color);

        return blob;
    }

    // Recursively expands the passed blob to all connected tiles
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
    // Returns true if the tile fell at all.
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
                // only fall if tile is in a different position than before
                if (rFall-1 != r) {
                    board[rFall-1, c] = board[r, c];
                    // I am subtracting half of width and height again here, because it only works tht way,
                    // i don't know enough about transforms to know why. bandaid solution moment.
                    board[rFall-1, c].transform.localPosition = new Vector3(c - 3.5f, -rFall + 1 + 6.5f, 0);

                    board[rFall-1, c].AnimateMovement(
                        new Vector2(0, (rFall-1)-r),
                        new Vector2(0, 0)
                    );

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

    public bool isPaused()
    {
        return pauseMenu.paused;
    }

    public bool isPostGame()
    {
        return postGame;
    }

    public void Defeat() 
    {
        postGame = true;
        defeated = true;
        Destroy(piece);
        pieceBoard.SetActive(false);
        Time.timeScale = 0f;
        winTextObj.SetActive(true);
        winText.text = "LOSE";
        enemyBoard.Win();

    }

    public void Win()
    {
        postGame = true;
        winTextObj.SetActive(true);
        winText.text = "WIN";
        winMenu.AppearWithDelay(2d);
    }

    public Tile[,] getBoard()
    {
        return this.board;
    }

    public int getColHeight(int col)
    {
        int l = 0;
        // loop through the given col, bottom to top.
        for (int i = board.GetLength(0)-1; i > 0; i--){
            // we have reached an empty tile aka top of the stack
            if (board[i,col] == null){
                l = height - i;
                break;
            }
        }
        // Debug.Log(l);
        return l;
    }

    public bool isInitialized()
    {
        return cycleInitialized;
    }

}
