using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random=UnityEngine.Random;

using ConvoSystem;
using SoloMode;
using PostGame;
using Animation;
using Battle.Cycle;
using Sound;

namespace Battle.Board {
    public class GameBoard : MonoBehaviour
    {


        // Everything else
        // If this board is in single player mode */
        [SerializeField] public bool singlePlayer;
        // The battler selected for this board.
        [SerializeField] private Battler battler;
        public Battler Battler {get {return battler;}}

        // True if the player controls this board. */
        [SerializeField] private bool playerControlled;
        /** 0 for left side, 1 for right side */
        [SerializeField] private int playerSide;

        /** Obect where pieces are drawn */
        [SerializeField] public GameObject pieceBoard;

        /** Input mapping for this board */
        [SerializeField] public InputScript[] inputScripts;

        /** Inputs that replace inputScripts list if solo mode */
        [SerializeField] public InputScript[] soloInputScripts;

        /** ControlsGraphic that will show the input keys */
        [SerializeField] public Battle.ControlsDisplaySystem.ControlsGraphic controlsGraphic;

        /** The board of the enemy of the player/enemy of this board */
        [SerializeField] public GameBoard enemyBoard;
        /** HP Bar game object on this board */
        [SerializeField] public HealthBar hpBar;

        /** Stores the piece preview for this board */
        [SerializeField] public PiecePreview piecePreview;

        /** Cycle pointer game object that belongs to this board */
        [SerializeField] public GameObject pointer;

        /** Stores the board's cycle level indicator */
        [SerializeField] private CycleMultiplier cycleLevelDisplay;
        /** Stores the image for the portrait */
        [SerializeField] public Image portrait;

        /** Chain popup object */
        [SerializeField] private Popup chainPopup;
        /** Cascade popup object */
        [SerializeField] private Popup cascadePopup;
        /** Attak popup object */
        [SerializeField] private Battle.AttackPopup attackPopup;
        /** Board background. Animated fall down when defeated */
        [SerializeField] private BoardDefeatFall boardDefeatFall;

        /** Current fall delay for pieces. */
        [SerializeField] private float fallTime = 0.8f;

        /** Extra time added to fallTime when piece is about to be placed. (Not affected by fallTimeMult) */
        [SerializeField] private float slideTime = 0.4f;

        /** Last time a piece was placed; new piece will not be placed within same as slideTime */
        private float lastPlaceTime;

        /** Win/lose text that appears over the board */
        [SerializeField] private GameObject winTextObj;
        private TMPro.TextMeshProUGUI winText;

        /** Starting HP of this character. */
        public int maxHp { get; private set; }
        /** Amount of HP this player has remaining. */
        public int hp { get; private set; }

        /// Amount of shield this battler has. As of rn Pyro is the only character to gain shield
        public int shield {get; private set;} = 0;
        public int maxShield {get; private set;} = 300;

        /** Stores the ManaCycle in this scene. (on start) */
        public ManaCycle cycle { get; private set; }

        /** This board's current position in the cycle. starts at 0 */
        public int cyclePosition { get; private set; }

        /** Dimensions of the board */
        public static readonly int width = 8;
        public static readonly int height = 18;
        // Visual size of the board; excludes top buffer rows incase piece is somehow moved up there; 
        // and starting position is probably there too
        public static readonly int physicalHeight = 14;

        /** The last time that the current piece fell down a tile. */
        private float previousFallTime;
        /** If this board is currently spellcasting (chaining). */
        private bool casting = false;

        /** Board containing all tiles that have been placed and their colors. NONE is an empty space (from ManaColor enum). */
        public Tile[,] tiles;
        /** Piece that is currently being dropped. */
        private Piece piece;

        /** Amount of times the player has cleared the cycle. Used in daamge formula */
        private int cycleLevel = 0;
        // Amount of boost this board gets from each cycle clear
        public int boostPerCycleClear {get; private set;} = 2;

        /** Set to false when piece falling is paused; game will not try to make the current piece fall, if there even is one */
        public bool doPieceFalling = true;

        /** If this board has had Defeat() called on it and in post game */
        private bool defeated;
        /** If this board has had Win() called on it and in post game */
        private bool won;

        private float fallTimeMult;

        // Used by this board's AI controller to check when a new piece has spawned. 
        // Set to false by the AI controller when acted upon
        public bool pieceSpawned;

        private bool cycleInitialized;
        private bool postGame = false;

        private Pause.PauseMenu pauseMenu;
        private PostGameMenu winMenu;

        /** Prefab for damage shoots, spawned when dealing damage. */
        [SerializeField] private GameObject damageShootPrefab;
        /** Can be used to shake the board. cached on start */
        private Shake shake;

        /** The level the player is in, if not in versus mode */
        [SerializeField] public Level level;
        /** If in singleplayer, the objective list in this scene */
        [SerializeField] private ObjectiveList objectiveList;
        /** Timer, to stop when this player wins **/
        [SerializeField] public Timer timer;
        /** Mid-level conversations that are yet to be shown; removed from list when shown */
        private List<MidLevelConversation> midLevelConvos;
        /** Convo handler in this scene; to play mid-level convos when available */
        [SerializeField] ConvoHandler convoHandler;

        /** If currently paused to show dialogue */
        public bool convoPaused;

        // STATS
        /** Total amount of mana this board has cleared */
        private int totalManaCleared;
        /** Total amount of spellcasts this player has performed */
        private int totalSpellcasts;
        /** Total amount of blobs currently lined up for the current color */
        private int totalBlobs;
        /** Highest combo scored by the player */
        private int highestCombo;

        // use gameobject for sounds so it can be saved as prefab and shared between boards
        [SerializeField] private GameObject sfxObject;
        private SFXDict.sfxDict serializedSoundDict;
        private Dictionary<string,AudioClip> sfx;

        public AudioClip multiBattleMusic;

        // Transform where particles are drawn to
        [SerializeField] private Transform particleParent;
        // Particle system for when a tile is cleared
        [SerializeField] private GameObject clearParticleSystem;

        // In party mode, this is the timer before the user takes direct uncounterable damage from all trash tiles
        // Timer starts when a trash tile is added, and will stop running when no trash tiles tick when timer is up
        private float trashDamageTimer;
        private static float trashDamageTimerDuration = 5f;
        private static int damagePerTrash = 5;

        // MANAGERS
        [SerializeField] public AbilityManager abilityManager;

        [SerializeField] public RngManager rngManager;

        // Start is called before the first frame update
        void Start()
        {
            // if in solo mode, add solo additional inputs
            if (Storage.gamemode == Storage.GameMode.Solo) inputScripts = soloInputScripts;

            // get sfx as regular dict
            serializedSoundDict = sfxObject.GetComponent<SFXDict>().sfxDictionary;
            sfx = serializedSoundDict.asDictionary();
            
            blobs = new List<Blob>();

            if (Storage.gamemode != Storage.GameMode.Default && playerSide == 0) {
                singlePlayer = (Storage.gamemode == Storage.GameMode.Solo);
            } else {
                singlePlayer = false;
            }

            // don't show controls for player2 if singleplayer and player 2
            if ((playerSide == 0 || !singlePlayer) && inputScripts.Length > 0 && inputScripts[0] != null) controlsGraphic.SetInputs(inputScripts[0]);

            // load level if applicable
            if (Storage.level != null)
            {
                level = Storage.level;
            }
            if (level != null) {
                midLevelConvos = new List<MidLevelConversation>(level.midLevelConversations);
            } else {
                midLevelConvos = new List<MidLevelConversation>();
            }

            if (playerSide == 0) {
                // Debug.Log("stopping bgm");
                // SoundManager.Instance.UnloadBGM();
                SoundManager.Instance.SetBGM(singlePlayer ? level.battleMusic : multiBattleMusic);
            }

            if (singlePlayer && !Storage.level.aiBattle) {
                // hp number is used as score, starts as 0
                maxHp = level.scoreGoal;
                hp = 0;
                if (enemyBoard != null) { enemyBoard.gameObject.SetActive(false); enemyBoard.pointer.SetActive(false); } 
                if (objectiveList != null) objectiveList.gameObject.SetActive(true);

                fallTime = level.fallTime;
            } else {
                // (Later, this may depend on the character/mode)
                maxHp = 2000;
                hp = maxHp;
                if (enemyBoard != null) enemyBoard.gameObject.SetActive(true);
                if (objectiveList != null) objectiveList.gameObject.SetActive(false);
            }

            // Cache stuff
            pauseMenu = GameObject.FindObjectOfType<Pause.PauseMenu>();

            winText = winTextObj.GetComponent<TMPro.TextMeshProUGUI>();
            winMenu = GameObject.Find("PostGameMenu").GetComponent<PostGameMenu>();

            shake = GetComponent<Shake>();

            // if any value in storage is null (and not solo), it means we loaded straight to ManaCycle without going to CharSelect first. use default serialized values for battlers
            if (Storage.battler1 != null && (Storage.gamemode != Storage.GameMode.Solo))
            {
                if (playerSide == 0)
                {
                    battler = Storage.battler1;
                    playerControlled = Storage.isPlayer1;
                }
                else
                {
                    battler = Storage.battler2;
                    playerControlled = Storage.isPlayer2;
                }
            }
            else
            {
                // if in solo mode, use battler and op serialized in level asset
                if (Storage.gamemode == Storage.GameMode.Solo && playerControlled)
                {
                    battler = level.battler;
                    // set opp in ai battles
                    if (Storage.level.aiBattle)
                    {
                        enemyBoard.battler = Storage.level.opponent;
                        enemyBoard.portrait.sprite = Storage.level.opponent.sprite;
                        if (playerSide == 0) enemyBoard.SetPlayerControlled(false);
                        
                    }
                }
            }

            // setup battler variables for the mirrored, take opponent sprite / abilities
            if (battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Shapeshifter)
            {
                battler = enemyBoard.battler;
                portrait.color = new Color(1f,0f,0f,0.47f);
                
            }

            portrait.sprite = battler.sprite;

            abilityManager.InitManaBar();

            SetShield(0);

            // initialPos = portrait.GetComponent<RectTransform>().anchoredPosition;
            portrait.GetComponent<RectTransform>().anchoredPosition = portrait.GetComponent<RectTransform>().anchoredPosition + battler.portraitOffset;
            attackPopup.SetBattler(battler);
            
            cyclePosition = 0;

            hpBar.Setup(this);

            if (singlePlayer && !Storage.level.aiBattle) {
                objectiveList.InitializeObjectiveListItems(this);
            }
        }

        void Update()
        {
            // temporarily in update() to find the correct values quicker, keeping in case needed again in the future
            // portrait.GetComponent<RectTransform>().anchoredPosition = initialPos + battler.portraitOffset;
            // wait for cycle to initialize (after countdown) to run game logic
            if (!cycleInitialized) return;

            PointerReposition();

            // -------- CONTROLS ----------------
            // if (inputScripts == null) return;
            foreach (InputScript inputScript in inputScripts) {
                if (inputScript != null)
                {
                    if (Input.GetKeyDown(inputScript.Pause) && !postGame && !Storage.convoEndedThisInput)
                    {
                        pauseMenu.TogglePause();
                        PlaySFX("pause");
                    }
                    Storage.convoEndedThisInput = false;

                    // control the pause menu if paused
                    if (pauseMenu.paused && !postGame)
                    {
                        if (Input.GetKeyDown(inputScript.Up)) {
                            pauseMenu.MoveCursor(1);
                            PlaySFX("move", pitch : 0.8f);
                        } else if (Input.GetKeyDown(inputScript.Down)) {
                            pauseMenu.MoveCursor(-1);
                            PlaySFX("move", pitch : 0.75f);
                        }

                        if (Input.GetKeyDown(inputScript.Cast)){
                            pauseMenu.SelectOption();
                        }           
                    }

                    // same with post game menu, if timer is not running
                    else if (postGame && !winMenu.timerRunning)
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
                    
                    // If not pausemenu paused, do piece movements if not dialogue paused and not in postgame
                    else if (!convoPaused && !postGame) {
                        if (playerControlled && piece != null){
                            if (Input.GetKey(inputScript.Down)){
                                this.fallTimeMult = 0.1f;
                            }
                            else{
                                this.fallTimeMult = 1f;
                            }
                        }
                    }
                }

                // -------- PIECE FALL/PLACE ----------------
                if (!defeated && doPieceFalling)
                {
                    // Get the time that has passed since the previous piece fall.
                    // If it is greater than fall time (or fallTime/10 if holding down),
                    // move the piece one down.
                    // (Final fall time has to be greater than 0.05)
                    double finalFallTime = fallTime*this.fallTimeMult;

                    // If not fast-dropping, slow down fall if this is a slow falling tile
                    if (piece && piece.slowFall && fallTimeMult > 0.2f) finalFallTime *= 2f;

                    if (finalFallTime < 0.05){
                        finalFallTime = 0.05;
                    }
                    if (finalFallTime < 0.4 && piece && piece.slowFall && fallTimeMult > 0.2f) {
                        finalFallTime = 0.4;
                    }

                    if(Time.time - previousFallTime > finalFallTime){

                        // Try to move the piece down.
                        bool movedDown = MovePiece(0, 1);

                        if (!movedDown) {
                            // If it can't be moved down,
                            // also check for sliding buffer, and place if beyond that
                            // don't use slide time if down held
                            // if (!Input.GetKey(inputScript.Down)) {
                            
                            // if (Input.GetKey(inputScript.Left) || Input.GetKey(inputScript.Right)) {
                
                                if (playerControlled && !Input.GetKey(inputScript.Down)) {
                                    finalFallTime += slideTime;
                                }


                            // true if time is up for the extra slide buffer
                            bool pastExtraSlide = Time.time - previousFallTime > finalFallTime;
                            // if exxtended time is up and still can't move down, place
                            if (pastExtraSlide && !movedDown && Time.time > lastPlaceTime + slideTime)
                            {
                                PlacePiece();
                            }
                        } else {
                            // If it did move down, adjust numbers.
                            // reset to 0 if row fallen to is below the last.
                            // otherwise, increment
                            if (piece != null && piece.GetRow() > lastRowFall) {
                                lastRowFall = piece.GetRow();
                                rowFallCount = 0;
                            } else {
                                rowFallCount++;
                                // if row fall count exceeds 3, auto place
                                if (rowFallCount > 3) {
                                    PlacePiece();
                                }
                            }

                            // if it did move, reset fall time
                            previousFallTime = Time.time;  
                        }
                    }    
                }

                // TRASH DAMAGE TIMER
                // if above 0, tick down
                if (trashDamageTimer > 0) {
                    trashDamageTimer -= Time.deltaTime;

                    // if reached 0, check for tiles.
                    
                    if (trashDamageTimer <= 0) {
                        int trashDamage = 0;
                        for (int r=0; r<height; r++) {
                            for (int c=0; c<width; c++) {
                                if (tiles[r, c] && tiles[r, c].trashTile) {
                                    trashDamage += damagePerTrash;
                                }
                            }
                        }

                        // if there are tiles, damage and reset the timer.
                        // if no tiles, set timer to 0 (not running)
                        if (trashDamage > 0) {
                            TakeDamage(trashDamage, 0.333f);
                            trashDamageTimer = trashDamageTimerDuration;
                        } else {
                            trashDamageTimer = 0;
                        }
                    }
                }
            }
        }

        // Initialize with a passed cycle. Taken out of start because it relies on ManaCycle's start method
        public void InitializeCycle(ManaCycle cycle)
        {
            if (!enabled) return;

            cycleInitialized = true;
            this.cycle = cycle;

            piecePreview.Setup(this);

            cyclePosition = 0;

            // hide enemy pointer if in single player and not ai battle
            if (playerSide == 0 && singlePlayer && !Storage.level.aiBattle) enemyBoard.pointer.SetActive(false);
            else enemyBoard.pointer.SetActive(true);
            // if (singlePlayer && !Storage.level.aiBattle) enemyBoard.pointer.SetActive(false);
            pointer.transform.SetParent(cycle.transform.parent);
            PointerReposition();

            tiles = new Tile[height, width];
            SpawnPiece();

            CheckMidLevelConversations();
        }


        public void RotateLeft(){
            if (!piece.IsRotatable) return;
            piece.RotateLeft();
            if(!ValidPlacement()){
                // try nudging left, then right, then up. If none work, undo the rotation
                // only bump up if row fallen to 3 or less times
                if (!MovePiece(-1, 0) && !MovePiece(1, 0) && !MovePiece(0, -1)) piece.RotateRight();
            }
            PlaySFX("rotate", pitch : Random.Range(0.75f,1.25f));
        }

        public void RotateRight(){
            if (!piece.IsRotatable) return;
            piece.RotateRight();
            if(!ValidPlacement()){
                // try nudging right, then left, then up. If none work, undo the rotation
                if (!MovePiece(1, 0) && !MovePiece(-1, 0) && !MovePiece(0, -1)) piece.RotateLeft();
            }
            PlaySFX("rotate", pitch : Random.Range(0.75f,1.25f));
        }

        public void MoveLeft(){
            MovePiece(-1, 0);
            PlaySFX("move", pitch : Random.Range(0.9f,1.1f));
        }

        public void MoveRight(){
            MovePiece(1, 0);
            PlaySFX("move", pitch : Random.Range(0.9f,1.1f));
        }

        public void Spellcast(){
            // get current mana color from cycle, and clear that color
            // start at chain of 1
            // canCast is true if a spellcast is currently in process.
            RefreshBlobs();
            if (!casting && blobs.Count != 0) {
                var shake = pointer.GetComponent<Shake>();
                if (shake != null) shake.StopShake();
                Spellcast(1);
            }
            else {
                PlaySFX("failedCast");
                var shake = pointer.GetComponent<Shake>();
                if (shake != null) shake.StartShake();
            }
        }

        public void UseAbility(){
            if (abilityManager.enabled) abilityManager.UseAbility();
        }

        public ManaColor PullColorFromBag() {
            return rngManager.PullColorFromBag();
        }

        public ManaColor GetCenterMatch() {
            return rngManager.GetCenterMatch();
        }


        // Adds a trash tile to this board in a random column.
        public void AddTrashTile() {
            /// Add a new trash piece with a random color
            SinglePiece trashPiece = Instantiate(abilityManager.singlePiecePrefab).GetComponent<SinglePiece>();
            trashPiece.GetCenter().SetColor(Piece.RandomColor(), this);
            trashPiece.GetCenter().MakeTrashTile(this);
            SpawnStandalonePiece(trashPiece);

            // start trash damage timer only if at 0 (not running)
            if (trashDamageTimer == 0) {
                trashDamageTimer = trashDamageTimerDuration;
            }
        }

        // Add a piece to this board without having the player control or place it (keep their current piece).
        public void SpawnStandalonePiece(Piece piece, int column) {
            // Send it to a random color and drop it
            piece.transform.SetParent(pieceBoard.transform, false);
            piece.MoveTo(column, 3);
            piece.PlaceTilesOnBoard(ref tiles, pieceBoard.transform);
            Destroy(piece);
            piece.OnPlace(this);
            foreach (Vector2Int pos in piece) {
                TileGravity(pos.x, pos.y);
            }
            RefreshBlobs();
            // may replace with trash sfx later
            PlaySFX("place");
        }

        // Spawn the standalone piece in a random column
        public void SpawnStandalonePiece(Piece piece) {
            SpawnStandalonePiece(piece, (int)Random.Range(0, 8));
        }

        // Hides all 

        public bool IsPlayerControlled(){
            return this.playerControlled;
        }

        public void SetPlayerControlled(bool p){
            playerControlled = p;
        }

        public bool IsDefeated(){
            return this.defeated;
        }

        public bool IsWinner() {
            return this.won;
        }

        // used by mid-level convo, to wait for spellcast to be done before convo.
        public bool WonAndNotCasting() {
            return this.won && !this.casting;
        }

        public Piece GetPiece(){
            return this.piece;
        }

        public void SetFallTimeMult(float m){
            this.fallTimeMult = m;
        }

        // The row that this piece last fell to.
        private int lastRowFall = 0;
        // The amount of times the piece has fallen to this row. 
        // If it falls to this row more than 3 times, it will auto-place.
        private int rowFallCount = 0;

        private void PlacePiece() {
            // Place the piece
            PlaceTilesOnBoard();

            // Move self damage cycle
            DamageCycle();

            RefreshObjectives();

            // If postgame, don't spawn a new piece
            if (postGame) {
                piece = null;
                return;
            }

            // Spawn a new piece & reset fall delay & row
            SpawnPiece();
            previousFallTime = Time.time;
        }
        
        /// <summary>
        /// Destroys the current piece and spawns the passed piece in its place.
        /// </summary>
        public void ReplacePiece(Piece nextPiece) {
            pieceSpawned = true;
            // destroy the piece currently being dropped
            piece.DestroyTiles();
            Destroy(piece);

            // parent the new piece and set it to the drop location
            nextPiece.transform.SetParent(pieceBoard.transform, false);
            nextPiece.MoveTo(3,4);

            // reset row fall data (used in sliding)
            lastRowFall = nextPiece.GetRow();
            rowFallCount = 0;

            piece = nextPiece;

            // If the piece is already in an invalid position, player has topped out
            if (!ValidPlacement()) {
                // set hp to 0 if not in endless
                if (!(level != null && level.time == -1)) hp = 0;
                
                piece.gameObject.SetActive(false);
                Defeat();
            }
        }

        // Update the pointer's cycle position.
        private void PointerReposition()
        {
            // Get the position of the ManColor the pointer is supposed to be on
            // Debug.Log(cycle);
            Transform manaColor = cycle.transform.GetChild(cyclePosition);
            // Debug.Log(cycle.transform.GetChild(cyclePosition));

            pointer.transform.position = manaColor.transform.position + Vector3.right * 50f * ((playerSide == 0) ? -1 : 1);
        }

        // Create a new piece and spawn it at the top of the board. Replaces the current piece field.
        public void SpawnPiece()
        {
            pieceSpawned = true;
            piece = piecePreview.SpawnNextPiece();
            lastRowFall = piece.GetRow();
            rowFallCount = 0;

            // If the piece is already in an invalid position, player has topped out
            if (!ValidPlacement()) {
                // set hp to 0 if not in endless
                if (!(level != null && level.time == -1)) hp = 0;
                
                piece.gameObject.SetActive(false);
                Defeat();
            }
        }

        // Move the current piece by this amount.
        // Return true if the piece is not blocked from moving to the new location.
        public bool MovePiece(int col, int row)
        {
            if (piece == null) return true;

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
                if (tiles[tile.y, tile.x] != null) return false;
            }
            // No tiles overlapped, return true
            return true;
        }

        // Place a piece on the grid, moving its Tiles into the board array and removing the Piece.
        public void PlaceTilesOnBoard()
        {
            if (piece == null) return;
            lastPlaceTime = Time.time;
            piece.PlaceTilesOnBoard(ref tiles, pieceBoard.transform);

            piece.OnPlace(this);

            // After tile objects are moved out, destroy the piece object as it is no longer needed
            Destroy(piece);

            if (!postGame) PlaySFX("place");

            // Keep looping until none of the piece's tiles fall
            // (No other tiles need to be checked as tiles underneath them won't move, only tiles above)
            bool tileFell = true;
            while (tileFell) {
                tileFell = false;
                
                // Affect all placed tiles with gravity.
                foreach (Vector2Int tile in piece)
                {
                    // Only do gravity if this tile is still here and hasn't fallen to gravity yet
                    if (!(tile.y >= height) && tiles[tile.y, tile.x] != null)
                    {
                        // If a tile fell, set tileFell to true and the loop will go again after this
                        if (TileGravity(tile.x, tile.y))
                        {
                            tileFell = true;
                        }
                    }
                }
            }

            RefreshBlobs();
            StartCoroutine(CheckMidConvoAfterDelay());
        }

        /// <summary>
        /// Clear the tile at the given index, destroying the Tile gameObject.
        /// Returns the point multiplier of the tile cleared.
        /// </summary>
        /// <param name="col">tile column</param>
        /// <param name="row">tile row</param>
        /// <returns>point multiplier of the cleared tile</returns>
        public float ClearTile(int col, int row, bool doParticleEffects, bool onlyClearFragile = false)
        {
            if (row < 0 || row >= height || col < 0 || col >= width) return 0;
            if (!tiles[row, col]) return 0;

            if (onlyClearFragile && !tiles[row, col].fragile) return 0;

            if (doParticleEffects) {
                // play clearing particles before clearing
                // instantiate particle system to have multiple bursts at once
                GameObject tileParticleSystem = Instantiate(clearParticleSystem, particleParent, false);

                // set particle color based on tile
                var particleSystem = tileParticleSystem.GetComponent<ParticleSystem>();
                var particleSystemMain = particleSystem.main;
                particleSystemMain.startColor = cycle.GetManaColors()[(int) tiles[row,col].GetManaColor()];

                // move to tile position and play burst
                // offset the x pos by 1 or -1 depending on side, idk why its offset like that /shrug
                // tileParticles.transform.localPosition = new Vector3(tiles[row,col].gameObject.GetComponent<RectTransform>().localPosition.x+(playerSide==0 ? 2 : -2), tiles[row,col].gameObject.GetComponent<RectTransform>().localPosition.y);
                tileParticleSystem.transform.position = tiles[row,col].transform.position;

                // Debug.Log(tileParticles + "at " + tiles[row,col].gameObject.GetComponent<RectTransform>().localPosition);
                // Debug.Log("actually at " + tileParticles.GetComponent<Transform>().position);
                particleSystem.Play();
                // particle system will automatically remove itself from the parent when animation is done, via ParticleSystem script
            }

            float pointMultiplier = tiles[row, col].pointMultiplier;

            // clear tile
            Destroy(tiles[row, col].gameObject);
            tiles[row, col] = null;

            // point multiplier should not be negative & lower damage
            return Math.Max(pointMultiplier, 0f);
        }

        public float ClearTile(int col, int row) {
            return ClearTile(col, row, true);
        }

        // Deal damage to the other player(s(?))
        // shootSpawnPos is where the shoot particle is spawned
        public void DealDamage(int damage, Vector3 shootSpawnPos, int color, int chain)
        {
            
            if (postGame) {
                // just add score if postgame and singleplayer
                if (singlePlayer && !Storage.level.aiBattle)
                {
                    hp += damage;   
                }
                // otherwise if versus, nothing will happen, other player is already dead so dont damage
                return;
            }

            // Spawn a new damageShoot
            GameObject shootObj = Instantiate(damageShootPrefab, shootSpawnPos, Quaternion.identity, transform);
            DamageShoot shoot = shootObj.GetComponent<DamageShoot>();
            shoot.damage = damage;

            if (chain >= 2) attackPopup.AttackAnimation();

            // Blend mana color with existing damage shoot color
            // var image = shootObj.GetComponent<Image>();
            // image.color = Color.Lerp(image.color, cycle.GetManaColor(color), 0.5f);

            // Send it to the appropriate location
            // if singleplayer, add damage to score, send towards hp bar
            if (singlePlayer && !Storage.level.aiBattle) {
                shoot.target = this;
                shoot.mode = DamageShoot.Mode.Heal;
                shoot.destination = hpBar.hpNum.transform.position;
            } 

            // if multiplayer, send damage to opponent
            else {
                // damage = hpBar.CounterIncoming(damage);
                // enemyBoard.EnqueueDamage(damage);
                // hpBar.Refresh();

                // move towards the closest damage
                // Iterate in reverse order; target closer daamges first
                for (int i=5; i>=0; i--)
                {
                    if (hpBar.DamageQueue[i].dmg > 0) {
                        shoot.target = this;
                        shoot.mode = DamageShoot.Mode.Countering;
                        shoot.destination = hpBar.DamageQueue[i].transform.position;
                        return;
                    }
                }

                // If the damage bar is empty and this fighter can make shields, make a shield if below max shield
                if (battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Shields && shield < maxShield) {
                    shoot.target = this;
                    shoot.mode = DamageShoot.Mode.Shielding;
                    shoot.destination = hpBar.shieldObject.transform.position;
                    return; 
                }

                //After shields, try to damage opponent's shield
                if (enemyBoard.shield > 0) {
                    shoot.target = enemyBoard;
                    shoot.mode = DamageShoot.Mode.AttackingEnemyShield;
                    shoot.destination = enemyBoard.hpBar.shieldObject.transform.position;
                    return;
                }

                // if no incoming damage/enemy shield was found, send straight to opponent
                shoot.target = enemyBoard;
                shoot.mode = DamageShoot.Mode.Attacking;
                shoot.destination = enemyBoard.hpBar.DamageQueue[0].transform.position;
            }
        }

        // Enqueues damage to this board.
        // Called from another board's DealDamage() method.
        public void EnqueueDamage(int damage)
        {
            hpBar.DamageQueue[0].AddDamage(damage);
            hpBar.Refresh();
            CheckMidLevelConversations();
        }

        // Moves incoming damage and take damage if at end
        public void DamageCycle()
        {
            // dequeue the closest damage
            int dmg = hpBar.DamageQueue[5].dmg;
            if (dmg > 0) {
                TakeDamage(dmg);
            }

            // advance queue
            hpBar.AdvanceDamageQueue();
        }

        public void TakeDamage(int damage, float intensity) {
            // shake the board and portrait when damaged
            shake.StartShake(intensity);
            portrait.GetComponent<Shake>().StartShake(intensity);
            // flash portrait red
            portrait.GetComponent<ColorFlash>().Flash(intensity);

            PlaySFX("damageTaken");

            // subtract from hp
            hp -= damage;
            hpBar.Refresh();

            // If this player is out of HP, run defeat
            if (hp <= 0) Defeat();

            CheckMidLevelConversations();
        }

        public void TakeDamage(int damage) {
            TakeDamage(damage, 1f);
        }

        public void UpdateShield() {
            hpBar.shieldNumText.text = shield+"";
            hpBar.shieldObject.SetActive( shield > 0 );
        }

        public void SetShield(int shield) {
            this.shield = shield;
            UpdateShield();
        }

        // Add shield to this board. Returns any leftover unaddable shield due to maximum reached.
        public int AddShield(int addShield) {
            shield += addShield;
            if (shield > maxShield) {
                int overflow = shield - maxShield;
                shield = maxShield;
                UpdateShield();
                return overflow;
            } else {
                UpdateShield();
                return 0;
            }
        }

        // Deal damage to the shield. If overdamaged, return overflow
        public int DamageShield(int damage) {
            shield -= damage;
            if (shield < 0) {
                int overflow = -shield;
                shield = 0;
                UpdateShield();
                return overflow;
            } else {
                UpdateShield();
                return 0;
            }
        }

        // Temporary, Only used for finding blobs within a single search, not used outside of search
        private static bool[,] tilesInBlobs;

        private static int minBlobSize = 3;
        /** Updated list of recognized blobs */
        public List<Blob> blobs;
        /** Total amount of mana in current blob list */
        private int totalBlobMana;

        public struct Blob
        {
            public ManaColor color;
            public List<Vector2Int> tiles;
        }


        /** Update the blob list this board has recognized. Should be called every time the board changes. */

        public void RefreshBlobs(ManaColor color) {
            tilesInBlobs = new bool[height, width];
            FindBlobs(color);
            totalBlobMana = TotalMana(blobs);
        }

        public void RefreshBlobs() {
            RefreshBlobs(CurrentColor());

            // just tucked this in here, since it correlates to the lot of the same things
            // refreshes zman color obscuring
            RefreshObscuredTiles();
        }

        // Check the board for blobs of 3 or more of the given color, and clear them from the board, earning points/dealing damage.
        private void Spellcast(int chain, bool refreshBlobs)
        {
            if (defeated) return;

            if (refreshBlobs) RefreshBlobs();

            // If there were no blobs, do not deal damage, and do not move forward in cycle, 
            // end spellcast if active
            // also end if defeated, combo shouldn't extend while dead
            if (blobs.Count == 0) {
                // Check for foresight icon, consuming it and skipping to next color if possible.
                if (abilityManager.ForesightCheck()) {
                    RefreshBlobs( cycle.GetColor( (cyclePosition+1) % ManaCycle.cycleLength ) );
                    if (totalBlobMana > 0) {
                        abilityManager.UseForesight();
                        AdvanceCycle();
                        Spellcast(chain);
                    } else {
                        RefreshBlobs( CurrentColor() );
                        casting = false;
                        RefreshObjectives();
                        StartCoroutine(CheckMidConvoAfterDelay());
                    }
                } else {
                    casting = false;
                    RefreshObjectives();
                    StartCoroutine(CheckMidConvoAfterDelay());
                }
                return;
            };

            // Keep clearing while mana cleared for current color is greater than 0
            // Begin spellcast
            int cascade = 0;
            casting = true;

            if (chain == 1) PlaySFX("startupCast");

            StartCoroutine(ClearCascadeWithDelay());
            IEnumerator ClearCascadeWithDelay()
            {
                // Brief delay before clearing current color
                yield return new WaitForSeconds(0.8f);

                if (!defeated) {
                    // Check for any new blobs that may have formed in those 0.8 seconds
                    // (Replaces old list)
                    // update: called on every place, so no longer needed here
                    // RefreshBlobs(color);

                    while (totalBlobMana > 0) {
                        // If this is cascading off the same color more than once, short delay between
                        if (cascade > 0) {
                            yield return new WaitForSeconds(0.5f);

                            if (defeated)

                            // recalculte blobs in case they changed
                            RefreshBlobs();
                        }

                        if (chain > 1) chainPopup.Flash(chain.ToString());

                        cascade += 1;
                        if (cascade > 1) cascadePopup.Flash(cascade.ToString());

                        // Get the average of all tile positions; this is where shoot particle is spawned
                        Vector3 averagePos = Vector3.zero;
                        foreach (Blob blob in blobs) {
                            for (int index = 0; index < blob.tiles.Count; index++) {
                                var pos = blob.tiles[index];
                                if (tiles[pos.y, pos.x] == null) {
                                    Debug.LogWarning("Null tile found in blob: "+pos.y+", "+pos.x);
                                    blob.tiles.RemoveAt(index);
                                    index--;
                                    totalBlobMana--;
                                    continue;
                                }
                                averagePos += tiles[pos.y, pos.x].transform.position;
                            }
                            
                        }
                        averagePos /= totalBlobMana;

                        totalSpellcasts++;
                        totalManaCleared += totalBlobMana;
                        if (abilityManager.enabled) abilityManager.GainMana(totalBlobMana);

                        highestCombo = Math.Max(highestCombo, chain);

                        float totalPointMult = 0;
                        // Clear all blob-contained tiles from the board.
                        foreach (Blob blob in blobs) {
                            // run onclear first to check for point multiplier increases (geo gold mine)
                            foreach (Vector2Int pos in blob.tiles) {
                                if (tiles[pos.y, pos.x]) tiles[pos.y, pos.x].BeforeClear(blob);
                            }
                            // then remove the tiles from the board and 
                            foreach (Vector2Int pos in blob.tiles) {
                                totalPointMult += ClearTile(pos.x, pos.y);

                                // clear adjacent fragile tiles
                                totalPointMult += ClearTile(pos.x - 1, pos.y, true, onlyClearFragile: true);
                                totalPointMult += ClearTile(pos.x + 1, pos.y, true, onlyClearFragile: true);
                                totalPointMult += ClearTile(pos.x, pos.y - 1, true, onlyClearFragile: true);
                                totalPointMult += ClearTile(pos.x, pos.y + 1, true, onlyClearFragile: true);
                            }
                        }
                        PlaySFX("cast1", pitch : 1f + chain*0.1f);

                        // Deal damage for the amount of mana cleared.
                        // DMG is scaled by chain and cascade.
                        int damage = (int)( (totalPointMult * damagePerMana) * (1 + (chain-1)*0.5f) * cascade );
                        // Send the damage over. Will counter incoming damage first.
                        DealDamage(damage, averagePos, (int)CurrentColor(), chain);

                        // Do gravity everywhere
                        AllTileGravity();

                        // Check for cascaded blobs (cascade loop will continue if resulting totalBlobMana > 0)
                        RefreshBlobs();

                        RefreshObjectives();
                        StartCoroutine(CheckMidConvoAfterDelay());
                    }

                    // Spellcast for the new next color and check for chain
                    AdvanceCycle();
                    Spellcast(chain+1);
                }            
            }
        }

        private void Spellcast(int chain) {
            Spellcast(chain, true);
        }

        private void AdvanceCycle() {
            // move to next in cycle position
            cyclePosition += 1;
            if (cyclePosition >= ManaCycle.cycleLength) {
                cyclePosition = 0;
                cycleLevel++;
                cycleLevelDisplay.Set(cycleLevel);
                PlaySFX("cycle");
            }
            PointerReposition();
        }

        public int damagePerMana {get {return 10 + cycleLevel*boostPerCycleClear;}}

        IEnumerator CheckMidConvoAfterDelay() {
            yield return new WaitForSeconds(0.4f);
            CheckMidLevelConversations();
        }

        // Updates list of all blobs of mana that were cleared.
        private void FindBlobs(ManaColor color)
        {
            blobs.Clear();

            // Loop over rows (top to bottom)
            for (int r = 0; r < height; r++)
            {
                // Loop over columns (left to right)
                for (int c = 0; c < width; c++)
                {
                    // Check if indexed tile exists and is correct color, and is not in a blob
                    if (tiles[r, c] != null && tiles[r, c].GetManaColor() == color && !tilesInBlobs[r, c])
                    {
                        Blob blob = CheckBlob(c, r, color);
                        if (blob.tiles.Count >= minBlobSize) blobs.Add(blob);
                    }
                }
            }
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
            if (tiles[r, c] == null) return;

            // Don't add if the tile is the incorrect color & this or target tile is not multicolor
            if (tiles[r, c].GetManaColor() != color 
            && color != ManaColor.Multicolor
            && tiles[r, c].GetManaColor() != ManaColor.Multicolor) return;

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
        public bool TileGravity(int c, int r, bool force = false)
        {
            // If there isn't a tile here, it can't fall, because it isnt a tile...
            if (tiles[r, c] == null) return false;

            // If the tile is an antigravity tile, do not pull it down. (etc. infinity's sword)
            // only pull downward if forced, e.g. by Infinity's ability itslef pulling it down
            // as opposed to from an AllTileGravity from a clear
            if (!tiles[r, c].doGravity && !force) return false;

            // For each tile, check down until there is no longer an empty tile
            for (int rFall = r+1; rFall <= height; rFall++)
            {
                // Once a non-empty is found, or reached the bottom move the tile to right above it
                if (rFall == height || tiles[rFall, c] != null)
                {
                    // only fall if tile is in a different position than before
                    if (rFall-1 != r) {
                        tiles[rFall-1, c] = tiles[r, c];
                        // I am subtracting half of width and height again here, because it only works tht way,
                        // i don't know enough about transforms to know why. bandaid solution moment.
                        tiles[rFall-1, c].transform.localPosition = new Vector3(
                            c - GameBoard.width/2f + 0.5f, 
                            -rFall + 1 + GameBoard.physicalHeight/2f - 0.5f + GameBoard.height - GameBoard.physicalHeight, 
                        0);

                        
                        tiles[rFall-1, c].AnimateMovement(
                            new Vector2(0, (rFall-1)-r),
                            new Vector2(0, 0)
                        );

                        tiles[r, c] = null;
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

        // Perform an action on all tiles.
        public void ForEachTile(Action<Tile> action)
        {
            // Loop over columns (left to right)
            for (int r = 0; r < height; r++)
            {
                // Loop over rows (BOTTOM to top)
                // Skip bottom tiles, as those cannot fall
                for (int c = 0; c < width; c++)
                {
                    if (tiles[r, c]) action(tiles[r, c]);
                }
            }
        }

        // Perform an action on all tiles, also passing their indexes.
        public void ForEachTileWithIndex(Action<Tile, int, int> action)
        {
            // Loop over columns (left to right)
            for (int r = 0; r < height; r++)
            {
                // Loop over rows (BOTTOM to top)
                // Skip bottom tiles, as those cannot fall
                for (int c = 0; c < width; c++)
                {
                    if (tiles[r, c]) action(tiles[r, c], r, c);
                }
            }
        }

        // Check if the tile at the index is the given color, if there is one there.
        public bool CheckColor(int r, int c, ManaColor color)
        {
            // return false if there is no tile at the index
            if (tiles[r,c] == null) return false;
            // if there is a tile, return true if it is the given color.
            return tiles[r,c].GetManaColor() == color;
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
            if (defeated || won) return;

            postGame = true;
            defeated = true;
            casting = false;
            hpBar.hpNum.gameObject.SetActive(false);
            if (timer != null) timer.StopTimer();
            foreach (var incoming in hpBar.DamageQueue) {
                incoming.SetDamage(0);
            }

            Destroy(piece);
            piece = null;

            // pieceBoard.SetActive(false);
            winTextObj.SetActive(true);
            winText.text = "LOSE";

            boardDefeatFall.StartFall();

            if (!singlePlayer) enemyBoard.Win();

            if (level != null) {
                winMenu.AppearAfterDelay(this);
                PlaySFX("lose");
                SoundManager.Instance.PauseBGM();
            }

            StartCoroutine(CheckMidConvoAfterDelay());
        }

        public void Win()
        {
            if (defeated || won) return;

            postGame = true;
            won = true;
            if (timer != null) timer.StopTimer();
            foreach (var incoming in hpBar.DamageQueue) {
                incoming.SetDamage(0);
            }

            winTextObj.SetActive(true);
            winText.text = "WIN";

            winMenu.AppearAfterDelay(this);

            StartCoroutine(CheckMidConvoAfterDelay());

            SoundManager.Instance.PauseBGM();
            PlaySFX("win");
        }

        /** Refreshed the objectives list. Will grant win to this player if all objectives met */
        public void RefreshObjectives() {
            // only check if in a level and are player 1
            if (playerSide == 0 && level != null) objectiveList.Refresh(this);
        }

        /** Checks for mid-level conversations that need to be displayed. return true if convo was played */
        public bool CheckMidLevelConversations() {
            // don't check if not player 1
            if (playerSide == 1) return false;

            // loop through and find the first with requirements met
            foreach (MidLevelConversation convo in midLevelConvos) {
                if (convo.ShouldAppear(this)) {
                    convoPaused = true;
                    Time.timeScale = 0;
                    convoHandler.StartConvo(convo, this);
                    midLevelConvos.Remove(convo);
                    // only one per check; return after, if any would be shown after this, they will be next check
                    return true;
                }
            }

            return false;
        }

        public Tile[,] getBoard()
        {
            return this.tiles;
        }

        public int getColHeight(int col)
        {
            int l = 0;
            // loop through the given col, bottom to top.
            for (int i = tiles.GetLength(0)-1; i > 0; i--){
                // we have reached an empty tile aka top of the stack
                if (tiles[i,col] == null){
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

        public bool IsCasting() {
            return casting;
        }

        public int GetTotalManaCleared() {
            return totalManaCleared;
        }

        public int GetTotalSpellcasts() {
            return totalSpellcasts;
        }

        public int GetHighestCombo() {
            return highestCombo;
        }

        public int GetBlobCount() {
            return blobs.Count;
        }

        public Level GetLevel() {
            return level;
        }

        public int GetPlayerSide(){
            return playerSide;
        }

        public bool GetCasting(){
            return casting;
        }

        // Used in singleplayer, add points to "score" (hp)
        public void AddScore(int score) {
            hp += score;
            hpBar.Refresh();
            RefreshObjectives();
            CheckMidLevelConversations();
        }

        public void PlaySFX(string value, float pitch = 1)
        {
            SoundManager.Instance.PlaySound(sfx[value], pitch : pitch);
        }

        static int obscureRadius = 3;
        static int obscureDistance = 4;

        // Recalculates which tiles are obscured by z?men
        public void RefreshObscuredTiles() {
            ForEachTile(tile => tile.Unobscure(this));

            
            ForEachTileWithIndex((tile, r, c) => {
                if (tile.obscuresColor) {
                    int obscureCount = 0;
                    for (int row = Math.Max(0, r-obscureRadius); row <= Math.Min(height-1, r+obscureRadius); row++) {
                        for (int col = Math.Max(0, c-obscureRadius); col <= Math.Min(width-1, c+obscureRadius); col++) {
                            // exclude tiles outside obscure distance, creates a curved border
                            if (Mathf.Abs(r-row) + Mathf.Abs(c-col) > obscureDistance) {
                                continue;
                            }

                            if (tiles[row, col]) {
                                tiles[row, col].Obscure();
                                obscureCount++;
                            }
                        }
                    }
                }
            });
        }
    }
}