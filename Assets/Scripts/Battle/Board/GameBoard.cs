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
        [SerializeField] private PiecePreview piecePreview;

        /** Cycle pointer game object that belongs to this board */
        [SerializeField] public GameObject pointer;

        /** Stores the board's cycle level indicator */
        [SerializeField] private CycleMultiplier cycleLevelDisplay;
        /** Stores the image for the portrait */
        [SerializeField] private Image portrait;

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
        public int boostPerCycleClear {get; private set;} = 3;

        /** Set to false when piece falling is paused; game will not try to make the current piece fall, if there even is one */
        public bool doPieceFalling = true;

        /** If this board has had Defeat() called on it and in post game */
        private bool defeated;
        /** If this board has had Win() called on it and in post game */
        private bool won;

        private float fallTimeMult;

        /** True for one frame when new piece spawns */
        private bool pieceSpawned;

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

        // MANAGERS
        [SerializeField] public AbilityManager abilityManager;

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

            if (singlePlayer) {
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
                // if in solo mode, use battler serialized in level asset
                if (Storage.gamemode == Storage.GameMode.Solo) battler = level.battler;
            }
            portrait.sprite = battler.sprite;
            // initialPos = portrait.GetComponent<RectTransform>().anchoredPosition;
            portrait.GetComponent<RectTransform>().anchoredPosition = portrait.GetComponent<RectTransform>().anchoredPosition + battler.portraitOffset;
            attackPopup.SetBattler(battler);
            
            cyclePosition = 0;

            hpBar.Setup(this);

            if (singlePlayer) {
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
            foreach (InputScript inputScript in inputScripts) {
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
                    pieceSpawned = false;

                    if (playerControlled && piece != null){
                        if (Input.GetKey(inputScript.Down)){
                            this.fallTimeMult = 0.1f;
                        }
                        else{
                            this.fallTimeMult = 1f;
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
                    if (finalFallTime < 0.05){
                        finalFallTime = 0.05;
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
                
                                if (!Input.GetKey(inputScript.Down)) {
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
            if (playerSide == 0 || !enemyBoard.singlePlayer) pointer.SetActive(true);
            else pointer.SetActive(false);
            if (singlePlayer) enemyBoard.pointer.SetActive(false);
            pointer.transform.SetParent(cycle.transform.parent);
            PointerReposition();

            tiles = new Tile[height, width];
            SpawnPiece();

            CheckMidLevelConversations();
        }


        public void RotateLeft(){
            piece.RotateLeft();
            if(!ValidPlacement()){
                // try nudging left, then right, then up. If none work, undo the rotation
                // only bump up if row fallen to 3 or less times
                if (!MovePiece(-1, 0) && !MovePiece(1, 0) && !MovePiece(0, -1)) piece.RotateRight();
            }
            PlaySFX("rotate", pitch : Random.Range(0.75f,1.25f));
        }

        public void RotateRight(){
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

        public bool isPlayerControlled(){
            return this.playerControlled;
        }

        public bool isDefeated(){
            return this.defeated;
        }

        public bool isWinner() {
            return this.won;
        }

        // used by mid-level convo, to wait for spellcast to be done before convo.
        public bool wonAndNotCasting() {
            return this.won && !this.casting;
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
            piece.DestroyTiles();
            Destroy(piece);


            nextPiece.transform.SetParent(pieceBoard.transform, false);
            nextPiece.MoveTo(3,4);

            lastRowFall = nextPiece.GetRow();
            rowFallCount = 0;

            piece = nextPiece;
        }

        // Update the pointer's cycle position.
        private void PointerReposition()
        {
            // Get the position of the ManColor the pointer is supposed to be on
            // Debug.Log(cycle);
            Transform manaColor = cycle.transform.GetChild(cyclePosition);
            // Debug.Log(cycle.transform.GetChild(cyclePosition));

            pointer.transform.position = new Vector3(
                // Move left or right based on if this is the player or not
                manaColor.transform.position.x + ((playerSide == 0) ? -50 : 50),
                manaColor.transform.position.y,
                0
            );
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
                    if (tiles[tile.y, tile.x] != null)
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

        // Clear the tile at the given index, destroying the Tile gameObject.
        public void ClearTile(int col, int row)
        {
            if (tiles[row, col]) {
                Destroy(tiles[row, col].gameObject);
                tiles[row, col] = null;
            }
        }

        // Deal damage to the other player(s(?))
        // shootSpawnPos is where the shoot particle is spawned
        public void DealDamage(int damage, Vector3 shootSpawnPos, int color, int chain)
        {
            
            if (postGame) {
                // just add score if postgame and singleplayer
                if (singlePlayer)
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
            if (singlePlayer) {
                shoot.target = this;
                shoot.countering = false;
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
                        shoot.countering = true;
                        shoot.destination = hpBar.DamageQueue[i].transform.position;
                        return;
                    }
                }

                // if no incoming damage was found, send straight to opponent
                shoot.target = enemyBoard;
                shoot.countering = false;
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
                // shake the board and portrait when damaged
                shake.StartShake();
                portrait.GetComponent<Shake>().StartShake();
                // flash portrait red
                portrait.GetComponent<ColorFlash>().Flash();

                PlaySFX("damageTaken");

                // subtract from hp
                hp -= dmg;

                // If this player is out of HP, run defeat
                if (hp <= 0) Defeat();

                CheckMidLevelConversations();
            }

            // advance queue
            hpBar.AdvanceDamageQueue();
        }


        // Temporary, Only used for finding blobs within a single search, not used outside of search
        private static bool[,] tilesInBlobs;

        struct Blob
        {
            public ManaColor color;
            public List<Vector2Int> tiles;
        }

        private static int minBlobSize = 3;
        /** Updated list of recognized blobs */
        private List<Blob> blobs;
        /** Total amount of mana in current blob list */
        private int totalBlobMana;


        /** Update the blob list this board has recognized. Should be called every time the board changes. */

        public void RefreshBlobs() {
            tilesInBlobs = new bool[height, width];
            FindBlobs();
            totalBlobMana = TotalMana(blobs);
        }

        // Check the board for blobs of 3 or more of the given color, and clear them from the board, earning points/dealing damage.
        private void Spellcast(int chain)
        {
            RefreshBlobs();

            // If there were no blobs, do not deal damage, and do not move forward in cycle, 
            // end spellcast if active
            // also end if defeated, combo shouldn't extend while dead
            if (defeated || blobs.Count == 0) {
                casting = false;
                RefreshObjectives();
                StartCoroutine(CheckMidConvoAfterDelay());
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

                        // DMG per mana, starts at 10, increases by 3 per cycle level
                        
                        // Deal damage for the amount of mana cleared.
                        // DMG is scaled by chain and cascade.
                        int damage = (int)( (totalBlobMana * damagePerMana) * chain * cascade );

                        // Get the average of all tile positions; this is where shoot particle is spawned
                        Vector3 averagePos = Vector3.zero;
                        foreach (Blob blob in blobs) {
                            foreach (Vector2Int pos in blob.tiles) {
                                averagePos += tiles[pos.y, pos.x].transform.position;
                            }
                        }
                        averagePos /= totalBlobMana;

                        // Send the damage over. Will counter incoming damage first.
                        DealDamage(damage, averagePos, (int)CurrentColor(), chain);

                        totalSpellcasts++;
                        totalManaCleared += totalBlobMana;
                        if (abilityManager.enabled) abilityManager.GainMana(totalBlobMana);

                        highestCombo = Math.Max(highestCombo, chain);

                        // Clear all blob-contained tiles from the board.
                        foreach (Blob blob in blobs) {
                            foreach (Vector2Int pos in blob.tiles) {
                                ClearTile(pos.x, pos.y);
                            }
                        }
                        PlaySFX("cast1", pitch : 1f + chain*0.1f);

                        // Do gravity everywhere
                        AllTileGravity();

                        // Check for cascaded blobs
                        RefreshBlobs();

                        RefreshObjectives();
                        StartCoroutine(CheckMidConvoAfterDelay());
                    }

                    // move to next in cycle position
                    cyclePosition += 1;
                    if (cyclePosition >= ManaCycle.cycleLength) {
                        cyclePosition = 0;
                        cycleLevel++;
                        cycleLevelDisplay.Set(cycleLevel);
                        PlaySFX("cycle");
                    }
                    PointerReposition();

                    // Spellcast for the new next color to check for chain
                    Spellcast(chain+1);
                }            
            }
        }

        public int damagePerMana {get {return 10 + cycleLevel * boostPerCycleClear;}}

        IEnumerator CheckMidConvoAfterDelay() {
            yield return new WaitForSeconds(0.4f);
            CheckMidLevelConversations();
        }

        // Updates list of all blobs of mana that were cleared.
        private void FindBlobs()
        {
            blobs.Clear();

            var color = CurrentColor();

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

            // Don't add if the tile is the incorrect color
            if (tiles[r, c].GetManaColor() != color) return;

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
            if (tiles[r, c] == null) return false;

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
    }
}