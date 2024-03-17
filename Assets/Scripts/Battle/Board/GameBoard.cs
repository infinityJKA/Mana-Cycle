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
using Battle.AI;

// may be a bad idea to namespace this, could be changed later
using static ArcadeStats.Stat;
using TMPro;
using Steamworks;
//using static Item.DeferType;

namespace Battle.Board {
    public class GameBoard : MonoBehaviour
    {
        // True if this is in the mobile mana cycle scene.
        // Changes various things such as hiding control display, pointer direction, etc
        [SerializeField] private bool mobile;
        public bool Mobile { get { return mobile;} }

        /// <summary>
        /// If this board is in single player mode
        /// </summary>
        [SerializeField] public bool singlePlayer;
        /// <summary>
        /// The battler selected for this board.
        /// </summary>
        [SerializeField] private Battler battler;
        public Battler Battler {get {return battler;}}

        /// <summary>
        /// True if the player controls this board.
        /// </summary>
        [SerializeField] private bool playerControlled;
        /// <summary>
        /// True if an AI should control this board.
        /// </summary>
        [SerializeField] private bool aiControlled;

        /// <summary>0 for left side, 1 for right side</summary>
        [SerializeField] private int playerSide;

        /// <summary>
        /// Obect where pieces are drawn
        /// </summary>
        [SerializeField] public Transform pieceBoard;

        /// <summary>
        /// Transform where ghost tiles are parented to
        /// </summary>
        [SerializeField] public Transform ghostPieceBoard;

        /// <summary>
        /// Input mapping for this board (unused as of 1.2.1)
        /// </summary>
        [SerializeField] public InputScript[] inputScripts;

        /// <summary>
        /// Inputs that replace inputScripts list if solo mode (unused as of 1.2.1)
        /// </summary>
        [SerializeField] public InputScript[] soloInputScripts;

        // ControlsGraphic that will show the input keys
        // [SerializeField] public Battle.ControlsDisplaySystem.ControlsGraphic controlsGraphic;

        /// <summary>
        /// The board of the enemy of the player/enemy of this board
        /// </summary>
        [SerializeField] public GameBoard enemyBoard;

        /// <summary>
        /// HP Bar game object on this board
        /// </summary>
        [SerializeField] public HealthBar hpBar;

        /// <summary>
        /// The piece preview managing component for this board
        /// </summary>
        [SerializeField] public PiecePreview piecePreview;

        /// <summary>
        /// Cycle pointer game object that belongs to this board
        /// </summary>
        [SerializeField] public GameObject pointer;
        [SerializeField] private Vector3 pointerOffset;

        /** Stores the board's cycle level indicator */
        [SerializeField] private CycleMultiplier cycleLevelDisplay;
        /** Stores the image for the portrait */
        [SerializeField] public Image portrait;

        /** Chain popup object */
        [SerializeField] private Animation.Popup chainPopup;
        /** Cascade popup object */
        [SerializeField] private Animation.Popup cascadePopup;
        /** Attak popup object */
        [SerializeField] private Battle.AttackPopup attackPopup;
        /** Board background. Animated fall down when defeated */
        [SerializeField] private BoardDefeatFall boardDefeatFall;

        /// If the board is inputting to quick fall the current piece. 
        private bool _quickFall = false;
        public bool quickFall {
            get {
                return _quickFall;
            }
            set {
                bool prev = _quickFall;
                _quickFall = value;
                if (prev != _quickFall && Storage.online && netPlayer.isOwned) {
                    netPlayer.CmdSetQuickfall(_quickFall);
                }
            }
        }

        /// If the board has inputted to insta-drop, but only true for one frame until after checked.
        public bool instaDropThisFrame = false;

        /// Whether or not the ghost tile should be drawn on this board
        public bool drawGhostPiece = true;
        /// <summary>If true, mana connected to the ghost piece will be lit up.</summary>
        public bool lightConnectedMana = true;

        /** Current fall delay for pieces. */
        [SerializeField] private float fallTime = 0.8f;

        /** Extra time added to fallTime when piece is about to be placed. (Not affected by fallTimeMult) */
        [SerializeField] private float slideTime = 0.4f;

        /** Last time a piece was placed; new piece will not be placed within same as slideTime */
        private float lastPlaceTime;

        /** Win/lose text that appears over the board */
        [SerializeField] private TMPro.TextMeshProUGUI winText;

        /** Starting HP of this character. */
        public int maxHp { get; private set; }
        /** Amount of HP this player has remaining. */
        public int hp { get; private set; }

        /// Amount of shield this battler has. As of rn Pyro is the only character to gain shield
        public int shield {get; private set;} = 0;
        public int maxShield { get { return 300 + 50 * cycleLevel; } }

        /** Stores the ManaCycle in this scene. (on start) */
        public ManaCycle cycle { get; private set; }

        /** This board's current position in the cycle. starts at 0 */
        public int cyclePosition { get; private set; }

        /** Dimensions of the board */
        public const int width = 8;
        public const int height = 20;
        // Visual size of the board; excludes top buffer rows incase piece is somehow moved up there; 
        // and starting position is probably there too
        public const int physicalHeight = 16;

        /** The last time that the current piece fell down a tile. */
        private float previousFallTime;
        /** If this board is currently spellcasting (chaining). */
        private bool casting = false;

        /** Board containing all tiles that have been placed and their colors. NONE is an empty space (from ManaColor enum). */
        public Tile[,] tiles;
        /// <summary>Non-real tiles that are simulated for the ghost piece and to light up connected mana</summary>
        private Tile[,] simulatedTiles;

        /** Piece that is currently being dropped. */
        private Piece piece;

        /// <summary>
        /// Ghost tiles that are being drawn to show where the current piece's tiles will land
        /// </summary>
        private List<Tile> ghostTiles;

        /** Amount of times the player has cleared the cycle. Used in damage formula */
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

        private bool battleStarted;
        public bool postGame { get; private set; } = false;

        public Pause.PauseMenu pauseMenu;
        public PostGameMenu winMenu;

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
        /** Highest combo scored by the player */
        private int highestCombo;
        /** Highest combo scored by the player */
        private int highestCascade;
        /** Total amount of spellcasts this player has performed from manual key presses */
        private int totalManualSpellcasts;
        /** Highest single damage spellcast achieved during the battle **/
        public int highestSingleDamage { get; private set; }

        public AudioClip[] usableBattleMusic;

        // Transform where particles are drawn to
        [SerializeField] private Transform particleParent;
        // Particle system for when a tile is cleared
        [SerializeField] private GameObject clearParticleSystem;
        public GameObject clearParticlePrefab => clearParticleSystem;

        // AI controller connected to this board for non-human opponents.
        [SerializeField] private AIController aiController;

        // In party mode, this is the timer before the user takes direct uncounterable damage from all trash tiles
        // Timer starts when a trash tile is added, and will stop running when no trash tiles tick when timer is up
        private float trashDamageTimer;


        // Amount of remaining lives. When dying, lose a life. If more than one life is remaining,
        // clear all incoming damage, clear the board, and suffer a 6-second delay before you can place pieces again.
        public int lives { get; private set; } = 1;
        // Transform where LifeHearts display the amount of remaining lives.
        [SerializeField] private Transform lifeTransform;
        // Prefab for a life heart displayed in the lives transform of the board
        [SerializeField] private GameObject lifeHeartObj;

        // Board background image - used to change color when recovering
        [SerializeField] private Image boardBackground;
        // standard board backgorund color, and color when recovering
        [SerializeField] private Color boardColor, boardRecoverColor;
        // true while in "recovery mode" - 5 seconds of delay, pieces will not spawn
        public bool recoveryMode { get; private set; }
        // Recovery timer - amount of time to wait before regaining control and pieces start spawning again
        private float recoveryTimer;
        // the number in the center of the board when in recovery mode
        [SerializeField] private TMPro.TextMeshProUGUI recoveryText;

        private static readonly float recoveryDelay = 3f;

        // MANAGERS
        [SerializeField] public AbilityManager abilityManager;

        [SerializeField] public RngManager rngManager;

        // dict for some game stats, set depending on mode
        public Dictionary<ArcadeStats.Stat, float> boardStats;
        public List<Item> equiped = new List<Item>();

        [SerializeField] private List<ColorFader> cycleColoredObjects;

        [SerializeField] private GameObject moveSFX, rotateSFX, castSFX, fullCycleSFX, loseSFX, PauseSFX, winSFX, failedCastSFX, placeSFX, damageTakenSFX, startupCastSFX, cascadeSFX, dmgShootSFX;

        [SerializeField] private TMP_Text usernameLabel;

        // online mode- the netplayer that controls this board
        public NetPlayer netPlayer {get; private set;}

        public void SetNetPlayer(NetPlayer netPlayer) {
            this.netPlayer = netPlayer;
            
            SetUsername(netPlayer.username);
        }

        // if piece will auto place when sliding against the ground for too long.
        // turned on when this is a board connected by an online opponent
        // to make srue pieces aren't placed in the wrong place
        private bool autoPlaceTiles = true;

        // If spellcast will automatically continue.
        // Enabled when this is an online opponent.
        // If spellcasts happened automatically they could desync if not in sync
        // with enemy's piece placements, so piece clear RPCs are sent in order with piece placement RPCs
        public bool manualCastContinue;

        // used in online mode to assert that order of placements/events has not been jumbled
        [SerializeField] private BoardSyncAssertion boardSync;
        public BoardSyncAssertion BoardSync => boardSync;

        /// <summary>
        /// Keeps track of the total amount of pieces this board has dropped, replaced, or destroyed.
        /// Used for board sync assertion in online multiplayer to help ensure piece drop order isn't jumbled
        /// </summary>
        public int pieceDropIndex {get; private set;}
        
        [SerializeField] private BoardCosmeticAssets _cosmetics;
        public BoardCosmeticAssets cosmetics => _cosmetics;

        // Start is called before the first frame update
        void Start()
        {
            // serialize lastSeriesLevel, if applicable 
            if (Storage.level != null && Storage.level.nextSeriesLevel != null) Storage.level.nextSeriesLevel.lastSeriesLevel = Storage.level;
            
            // if in solo mode, add solo additional inputs
            // also use in player vs. ai
            if ((Storage.gamemode == Storage.GameMode.Solo) 
            || (Storage.gamemode == Storage.GameMode.Versus && !Storage.isPlayerControlled2)) inputScripts = soloInputScripts;
            
            blobs = new List<Blob>();

            if (Storage.level && Storage.level.generateNextLevel) equiped = ArcadeStats.equipedItems;

            if (playerSide == 0) {
                singlePlayer = Storage.gamemode == Storage.GameMode.Solo;
            } else {
                singlePlayer = false;
            }

            // don't show controls for player2 if singleplayer and player 2
            // if (mobile) controlsGraphic.gameObject.SetActive(false);
            // else if ((playerSide == 0 || !singlePlayer) && inputScripts.Length > 0 && inputScripts[0] != null) controlsGraphic.SetInputs(inputScripts[0]);

            // load level if applicable
            if (Storage.level != null)
            {
                level = Storage.level;
            }
            if (level != null && level.midLevelConversations != null) {
                midLevelConvos = new List<MidLevelConversation>(level.midLevelConversations);
            } else {
                midLevelConvos = new List<MidLevelConversation>();
            }

            // if (playerSide == 1) boardStats = ArcadeStats.defaultStats;
            // Debug.Log("BOOST PER CLEAR IS " + boostPerCycleClear);

            if (playerSide == 0) {
                SoundManager.Instance.LoadBGM(singlePlayer ? level.battleMusic : usableBattleMusic[Random.Range(0, usableBattleMusic.Length - 1)]);
                // wait until after countdown to play mus
                // SoundManager.Instance.PauseBGM();
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

            // load stats dict
            if (Storage.level == null || !Storage.level.generateNextLevel)
            {
                // if not in arcade endless, use default stats
                boardStats = ArcadeStats.defaultStats;
                // Debug.Log(string.Join("\n",boardStats));
            }
            else 
            {
                // otherwise, use player stats
                boardStats = (playerSide == 0) ? ArcadeStats.playerStats : Storage.level.enemyStats;
                if (Storage.level.lastSeriesLevel != null) maxHp = (playerSide == 0) ? ArcadeStats.maxHp : Storage.level.enemyHp;
                hp = maxHp;
                Debug.Log(string.Join("\n",boardStats));
                Debug.Log(string.Join("\n",ArcadeStats.playerStats));
                Debug.Log("This is loading the player stats, not default. " + playerSide);

            }

            boostPerCycleClear = (int) (boardStats[CycleMultIncrease] * 10);

            shake = GetComponent<Shake>();

            // if any value in storage is null (and not solo), it means we loaded straight to ManaCycle without going to CharSelect first. use default serialized values for battlers
            if (Storage.battler1 != null && (Storage.gamemode != Storage.GameMode.Solo))
            {
                if (playerSide == 0)
                {
                    battler = Storage.battler1;
                    playerControlled = Storage.isPlayerControlled1;
                    aiControlled = !Storage.isPlayerControlled1;
                }
                else
                {
                    battler = Storage.battler2;
                    playerControlled = Storage.isPlayerControlled2;
                    aiControlled = !Storage.isPlayerControlled2;
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
                        Storage.battler2 = Storage.level.opponent;
                        enemyBoard.battler = Storage.level.opponent;
                        enemyBoard.portrait.sprite = Storage.level.opponent.sprite;
                        if (playerSide == 0) enemyBoard.SetPlayerControlled(false);
                        enemyBoard.SetAIDifficulty(Storage.level.aiDifficulty);
                    }
                }
            }

            InitBattler();

            // in versus mode player vs ai or ai vs ai, set difficulty levels
            if (Storage.gamemode == Storage.GameMode.Versus) {
                // AI vs. AI
                if (!Storage.isPlayerControlled1 && !Storage.isPlayerControlled2) {
                    SetAIDifficulty(FBPP.GetInt(playerSide == 0 ? "CpuVsCpuP1Level" : "CpuVsCpuP2Level", 5)/10f);
                }

                // player vs AI - set on player 2 only
                else if (Storage.isPlayerControlled1 && !Storage.isPlayerControlled2 && playerSide == 1) {
                    SetAIDifficulty(FBPP.GetInt("CpuLevel", 5)/10f);
                }
            }
            
            cyclePosition = 0;

            SetShield(0);

            hpBar.Setup(this);

            if (singlePlayer && !Storage.level.aiBattle) {
                objectiveList.InitializeObjectiveListItems(this);
            }

            drawGhostPiece = playerControlled && FBPP.GetInt(playerSide == 0 ? "drawGhostPiece" : "drawGhostPieceP2", 1) == 1;
            ghostTiles = new List<Tile>();

            abilityManager.enabled = FBPP.GetInt("enableAbilities", 1) == 1;
            abilityManager.InitManaBar();

            recoveryText.enabled = false;

            // in versus mode or solo mode player, use val from storage
            if (Storage.gamemode == Storage.GameMode.Versus || (Storage.gamemode == Storage.GameMode.Solo && playerSide == 0)) {
                lives = Storage.lives;
                // also persist HP if moving on in arcade mode
                if (Storage.gamemode == Storage.GameMode.Solo && Storage.level.lastSeriesLevel) {
                    hp = Storage.hp;
                    hpBar.Refresh();
                }
            } 
            // enemies in solo mode levels will always have 1 life (might make param for this in level i fneeded)
            else if (Storage.gamemode == Storage.GameMode.Solo && playerSide == 1) {
                lives = 1;
            } // otherwise, def. value of 3 will remain which is universal default

            // If in solo mode non-arcade or versus mode, hide lives list if only 1 life
            // also hide if arcade mode enemy
            if (Storage.gamemode == Storage.GameMode.Versus || Storage.level && (Storage.level.nextSeriesLevel || Storage.level.generateNextLevel || Storage.level.lastSeriesLevel) && playerSide == 0) {
                // when the game starts, have the life transform mirror the amount of lives
                foreach (Transform child in lifeTransform) Destroy(child.gameObject);
                for (int i=0; i<lives; i++) {
                    Instantiate(lifeHeartObj, lifeTransform);
                }
            } else {
                lifeTransform.gameObject.SetActive(false);
            }

            tilesInBlobs = new bool[height, width];

            // z?man starts with increased cycle
            if(battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Osmose){
                cycleLevel++;
            }

            cycleLevelDisplay.Set(cycleLevel);

        } // close Start()

        void InitBattler() {
            portrait.sprite = battler.sprite;
            portrait.GetComponent<RectTransform>().anchoredPosition = portrait.GetComponent<RectTransform>().anchoredPosition + battler.portraitOffset;
            attackPopup.SetBattler(battler);
        }

        void SetAIDifficulty(float difficulty) {
            aiController.difficulty = difficulty;
            // ai shoudn't bee too dumb or it will just top out and be boring, accuracy shuld actually stay high, speed is what matters
            aiController.accuracy = Mathf.Lerp(0.5f, 1f, difficulty);

            // gonna keep these static for now
            aiController.castChanceMultiplier = 1.1f;
            aiController.abilityChanceMultiplier = 1f;

            if (difficulty == 1f) {
                // max out speed on highest difficulty, ~25 moves per second, also enable concurrent actions
                aiController.moveDelay = 0.05f;
                aiController.concurrentActions = true;
            } else {
                float movesPerSecond = Mathf.Lerp(0.1f, 9.5f, difficulty);
                aiController.moveDelay = 1/movesPerSecond;
                aiController.concurrentActions = false;
            }
        }

        void Update()
        {
            // temporarily in update() to find the correct values quicker, keeping in case needed again in the future
            // portrait.GetComponent<RectTransform>().anchoredPosition = initialPos + battler.portraitOffset;
            // wait for cycle to initialize (after countdown) to run game logic
            if (!IsBattleStarted() || isPaused() || isPostGame()) return;

            // TODO: this was recently commented out, make sure this doesn't break the game. removed for performance
            // PointerReposition();

            if (Application.isEditor && Input.GetKey(KeyCode.Alpha1) && playerSide == 0)
            {
                enemyBoard.TakeDamage(100);
            }

            if (Application.isEditor && Input.GetKey(KeyCode.Alpha2) && playerSide == 0)
            {
                abilityManager.GainMana(100);
            }

            if (recoveryMode) {
                recoveryTimer -= Time.deltaTime;
                if (recoveryTimer <= 0f) {
                    Respawn();
                } else {
                    recoveryText.text = Mathf.CeilToInt(recoveryTimer)+"";
                }
            }

            // TRASH DAMAGE TIMER
            // if above 0, tick down
            // If not in a level or level is against an AI, take trash damage
            // do not evaluate trash timer if this is an online opponent
            bool isOnlineOpponent = Storage.online && !netPlayer.isOwned;
            if (!isOnlineOpponent && !defeated && !postGame && trashDamageTimer > 0) {
                trashDamageTimer -= Time.deltaTime;
                Vector3 averagePos = Vector3.zero;

                // if reached 0, check for tiles.
                int trashTiles = 0;
                if (trashDamageTimer <= 0) {
                    for (int r=0; r<height; r++) {
                        for (int c=0; c<width; c++) {
                            if (tiles[r, c] && tiles[r, c].trashTile) {
                                trashTiles++;
                                averagePos += tiles[r,c].transform.position;
                            }
                        }
                    }

                    averagePos /= trashTiles;
                    int trashDamage = trashTiles * damagePerTrash;

                    // if there are tiles, damage and reset the timer.
                    if (trashDamage > 0) 
                    {
                        // in singleplayer, trash tiles earn points instead of deal damage to self
                        if (singlePlayer) {
                            DealDamage(trashDamage, averagePos, partOfChain: false);
                        } else {
                            TakeDamage(trashDamage, 0.333f, canDamageShield: true);
                        }
                        trashDamageTimer = trashDamageTimerDuration;

                        // send hp to opponent in online mode, since trash timer is nnot evaluated on their board.
                        if (Storage.online && netPlayer.isOwned) netPlayer.CmdUpdateHp(hp, intensity: 0.333f);
                    } 
                    // if no tiles, set timer to 0 (not running)
                    else 
                    {
                        trashDamageTimer = 0;
                    }
                }
            }

            // -------- CONTROLS ----------------
            // moved to COntroller.cs

            // -------- PIECE FALL/PLACE ----------------
            if (!defeated && !recoveryMode && doPieceFalling)
            {
                if (instaDropThisFrame && battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Instadrop) {
                    PlacePiece();
                } else {
                    fallTimeMult = quickFall ? boardStats[QuickDropSpeed] : 1f;

                    // Get the time that has passed since the previous piece fall.
                    // If it is greater than fall time (or fallTime/10 if holding down),
                    // move the piece one down.
                    // (Final fall time has to be greater than 0.05)
                    float finalFallTime = fallTime*this.fallTimeMult;

                    // If not fast-dropping, slow down fall if this is a slow falling tile
                    if (piece && piece.slowFall && fallTimeMult > 0.2f) finalFallTime *= 2f;

                    if (finalFallTime < 0.05f){
                        finalFallTime = 0.05f;
                    }
                    if (finalFallTime < 0.8f && piece && piece.slowFall && !quickFall) {
                        finalFallTime = 0.8f;
                    }

                    if (Time.time - previousFallTime > finalFallTime) {

                        // Try to move the piece down.
                        bool movedDown = MovePiece(0, 1);

                        if (!movedDown && autoPlaceTiles) {
                            // If it can't be moved down,
                            // also check for sliding buffer, and place if beyond that
                            // don't use slide time if quick falling
                            if (!quickFall && level) {
                                finalFallTime += slideTime*level.slideTimeMult;
                            }

                            // true if time is up for the extra slide buffer
                            bool pastExtraSlide = (Time.time - previousFallTime) > finalFallTime;
                            // if exxtended time is up and still can't move down, place
                            if (pastExtraSlide && Time.time > lastPlaceTime + slideTime)
                            {
                                PlacePiece();
                            }
                        } else {
                            if (autoPlaceTiles) {
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
                            }

                            // if it did move, reset fall time
                            previousFallTime = Time.time;  
                        }
                    } 
                }

                instaDropThisFrame = false;
                Storage.convoEndedThisInput = false;
            }
        }

        private void Respawn() {
            boardBackground.color = boardColor;
            recoveryText.enabled = false;
            recoveryMode = false;
            defeated = false;
            hpBar.hpNum.gameObject.SetActive(true);
            hp = maxHp;
            hpBar.Refresh();
            previousFallTime = Time.time;
            if (!piece) SpawnPiece();
        }

        private static float trashDamageTimerDuration = 5f;
        private static int damagePerTrash = 5;

        /// <summary>
        /// Allows the first piece to begin falling another actions to occur when the countdown reaches 0.
        /// </summary>
        public void StartBattle()
        {
            battleStarted = true;

            SpawnPiece();
            pointer.SetActive(true);
            PointerReposition();

            CheckMidLevelConversations();
        }

        // Initialize with a passed cycle. Taken out of start because it relies on ManaCycle's start method
        public void InitializeWithCycle(ManaCycle cycle) {
            if (!enabled) {
                Debug.LogError("Trying to cycle initialize a disabled board!");
                return;
            };
            
            this.cycle = cycle;

            // Don't draw ghost tiles if this is a net controlled board
            if (Storage.online && !netPlayer.isOwned) drawGhostPiece = false;

            piecePreview.Setup(this);

            cyclePosition = 0;

            // hide enemy pointer if in single player and not ai battle
            if (playerSide == 0 && singlePlayer && !Storage.level.aiBattle) enemyBoard.pointer.SetActive(false);
            // if (singlePlayer && !Storage.level.aiBattle) enemyBoard.pointer.SetActive(false);

            tiles = new Tile[height, width];
            if (drawGhostPiece || lightConnectedMana) simulatedTiles = new Tile[height, width];

            // setup battler variables for the mirrored, take opponent sprite / abilities
            // taken out of start because it relies on enemy board to be properly set up
            if (battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Shapeshifter)
            {
                battler = enemyBoard.battler;
                InitBattler();
                portrait.color = new Color(0.9f,0.2f,0.1f,0.57f);
            }
            else
            {
                portrait.color = new Color(1f,1f,1f,0.47f);
            }


            // setup level trash timer if applicable
            if (Storage.gamemode == Storage.GameMode.Solo && level.trashSendRate > 0) Invoke("AddTrashTile", level.trashSendRate);

            portrait.GetComponent<ColorFlash>().SetBaseColor(portrait.color);
            portrait.sprite = battler.sprite;

            attackPopup.SetBattler(battler);

            abilityManager.InitManaBar();

            hpBar.Refresh();

            UpdateCycleColoredObjects();

            // turn off auto-placing and auto-chaining if this is an online opponent
            autoPlaceTiles = !Storage.online || netPlayer.isOwned;
            manualCastContinue = !Storage.online || netPlayer.isOwned;

            // move pointer to parent under cycle as a child under so it is not drawn below the cycle container
            pointer.transform.SetParent(cycle.transform.parent);
            // keep hidden before countdown reaches 0
            pointer.SetActive(false);
        }

        public void RotateCCW(){
            if (!IsBattleStarted() || isPaused() || isPostGame() || convoPaused) return;

            if (!piece.isRotatable) return;
            piece.RotateLeft();

            if(!ValidPlacement()){
                // try nudging left, then right, then up. If none work, undo the rotation
                // only bump up if row fallen to 3 or less times
                if (!MovePiece(-1, 0) && !MovePiece(1, 0) && !MovePiece(0, -1)) piece.RotateRight();
            }

            if (Storage.online && netPlayer.isOwned) NetworkUpdatePiece();

            RefreshGhostPiece();

            PlaySFX(rotateSFX);
        }

        public void RotateCW(){
            if (!IsBattleStarted() || isPaused() || isPostGame() || convoPaused) return;

            if (!piece.isRotatable) return;
            piece.RotateRight();

            if(!ValidPlacement()){
                // try nudging right, then left, then up. If none work, undo the rotation
                if (!MovePiece(1, 0) && !MovePiece(-1, 0) && !MovePiece(0, -1)) piece.RotateLeft();
            }

            if (Storage.online && netPlayer.isOwned) NetworkUpdatePiece();

            RefreshGhostPiece();

            PlaySFX(rotateSFX);
        }

        public bool MoveLeft(){
            if (!IsBattleStarted() || isPaused() || isPostGame() || convoPaused) return false;

            if (MovePiece(-1, 0)) {
                PlaySFX(moveSFX);
                return true;
            }
            return false;
        }

        public bool MoveRight() {
            if (!IsBattleStarted() || isPaused() || isPostGame() || convoPaused) return false;

            if (MovePiece(1, 0)) {
                PlaySFX(moveSFX);
                return true;
            }
            return false;
        }

        public void NetworkUpdatePiece() {
            netPlayer.CmdMovePiece(piece.GetCol(), piece.GetRotation());
        }

        public void SetPiecePosition(int col, int row) {
            piece.MoveTo(col, row);
        }

        /// <summary>
        /// Start a new spellcast chain off the current color. Is called when spellcast button is pressed
        /// Can also advance conversations if a midlevelconvo is being shown. /// 
        /// </summary>
        public void TrySpellcast(){
            if (!IsBattleStarted() || isPaused()) return;

            if (convoPaused) {
                convoHandler.Advance();
                return;
            }

            if (isPostGame()) return;

            // get current mana color from cycle, and clear that color
            // start at chain of 1
            // canCast is true if a spellcast is currently in process.
            RefreshBlobs();
            if (!casting && blobs.Count != 0) {
                var shake = pointer.GetComponent<Shake>();
                if (shake != null) shake.StopShake();
                chain = 1;
                cascade = 0;
                Spellcast(refreshBlobs: false);
                totalManualSpellcasts++;
            }
            else {
                PlaySFX(failedCastSFX);
                var shake = pointer.GetComponent<Shake>();
                if (shake != null) shake.StartShake();
            }
        }

        public void UseAbility(){
            if (!IsBattleStarted() || isPaused() || isPostGame() || convoPaused) return;

            if (abilityManager.enabled) {
                abilityManager.UseAbility();
            }
        }


        /// <summary>
        /// Adds a trash tile to this board in a random column.
        /// </summary>
        /// <param name="rng">rng to use for the trash tile's color</param>
        /// <param name="col">column to spawn in, -1 for random</param>
        /// <returns>the column it was spawned in.</returns>
        public int AddTrashTile(System.Random rng, int col = -1) {
            /// Add a new trash piece with a random color
            Piece trashPiece = Instantiate(abilityManager.singlePiecePrefab).GetComponent<Piece>();
            // give it a unique id
            trashPiece.id = piecePreview.NextPieceId();

            trashPiece.GetTile(0).SetManaColor(Piece.RandomColor(rng), this);
            trashPiece.GetTile(0).MakeTrashTile();

            // if col is -1, send to random tile, will return the column sent in
            if (col == -1) {
                col = SpawnStandalonePiece(trashPiece);
            } 
            // if not, send in the specified column
            else {
                SpawnStandalonePiece(trashPiece, col);
            }

            // start trash damage timer only if at 0 (not running)
            if (trashDamageTimer == 0) {
                trashDamageTimer = trashDamageTimerDuration;
            }

            // start trash timer again if applicable
            if (level != null && level.trashSendRate > 0) Invoke("AddTrashTile", level.trashSendRate);

            return col;
        }

        // Add a piece to this board without having the player control or place it (keep their current piece).
        public int SpawnStandalonePiece(Piece newPiece, int column) {
            // Send it to the passed column and drop it
            newPiece.transform.SetParent(pieceBoard, false);
            newPiece.MoveTo(column, 1);
            newPiece.PlaceTilesOnBoard(ref tiles, pieceBoard);
            Destroy(newPiece.gameObject);
            newPiece.OnPlace(this);

            // previousFallTime = Time.time;  
            foreach (Vector2Int pos in newPiece) {
                TileGravity(pos.x, pos.y);
            }
            RefreshBlobs();
            RefreshGhostPiece();
            // may replace with trash sfx later
            PlaySFX(placeSFX);
            return column;
        }

        // Spawn the standalone piece in a random column
        // Returns the column the piece was spawned in
        public int SpawnStandalonePiece(Piece piece) {
            // Choose a random column to send the piece to.
            // If at any point it overlaps with the piece being dropped, choose a new column
            // Try this a maximum of 40 times before giving up and destroying the piece
            // note: only checks for the center piece, aka single pieces/trash tiles
            for (int i=0; i<40; i++) {
                bool valid = true;
                int col = Random.Range(0, 8);

                int row = 1;
                while (row < height && !tiles[row, col]) {
                    row++;
                }

                foreach (var pos in piece) {
                    if (pos.y == row-1 && pos.x == col) {
                        valid = false;
                        break;
                    }
                }

                if (valid) {
                    SpawnStandalonePiece(piece, col);
                    return col;
                }
            }

            Debug.LogWarning("Standalone tile could not be placed");
            piece.DestroyTiles();
            Destroy(piece.gameObject);
            return -1;
        }

        // Hides all 

        public bool IsPlayerControlled(){
            return this.playerControlled;
        }

        public bool IsAiControlled(){
            return this.aiControlled;
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

        private int NextDropIndex() {
            pieceDropIndex++;
            return pieceDropIndex;
        }

        // The row that this piece last fell to.
        private int lastRowFall = 0;
        // The amount of times the piece has fallen to this row. 
        // If it falls to this row more than 3 times, it will auto-place.
        private int rowFallCount = 0;

        public void PlacePiece() {
            if (!piece) return;

            // Assert correct order in online mode
            // ensure the piece drop index is not lower than a drop index already received, this would mean a jumble occured
            // not sure if this is needed if the player owns this board but just in case for now
            // because if they desync, the owner of the board shouldnt receive a full board tile refresh from the client that doesn't own this board...
            // but ig its a good debug tool
            int dropIndex = NextDropIndex();
            if (boardSync) boardSync.AssertCorrectPieceDropOrder(dropIndex);

            // The piece will only advance the damage cycle when placed if it does not have a special ability
            bool advanceDamage = piece.effect == Battler.ActiveAbilityEffect.None;

            PlaceTilesOnBoard();

            if (advanceDamage) DamageCycle();

            // send placement and hp data to opponent in online mode
            if (Storage.online && netPlayer.isOwned) {
                netPlayer.CmdPlacePiece(piece.GetCol(), piece.GetRotation(), hp, piece.id, dropIndex);
            }

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
            if (!nextPiece) return;
            pieceSpawned = true;

            // destroy the piece currently being dropped
            piece.DestroyTiles();
            Destroy(piece.gameObject);

            // parent the new piece and set it to the drop location
            nextPiece.transform.SetParent(pieceBoard, false);
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

            pointer.transform.position = manaColor.transform.position + pointerOffset;
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
                return;
            }

            RefreshGhostPiece();
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

            // if the piece moved horizontally, play sound & refresh ghost piece
            if (col != 0) {
                RefreshGhostPiece();
            }

            // send movement data to the other player. only update if moving left or right
            if (Storage.online && col != 0 && netPlayer.isOwned) NetworkUpdatePiece();

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
            if (!piece) return;
            lastPlaceTime = Time.time;
            piece.PlaceTilesOnBoard(ref tiles, piece.ghostPiece ? ghostPieceBoard : pieceBoard.transform);

            if (!piece.ghostPiece) piece.OnPlace(this);

            // After tile objects are moved out, destroy the piece object as it is no longer needed
            Destroy(piece.gameObject);

            if (!postGame) PlaySFX(placeSFX);

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

        // Destroy any existing ghost tiles on the board (simulated board).
        void DestroyExistingGhostTiles() {
            foreach (var ghostTile in ghostTiles) Destroy(ghostTile.gameObject);
            ghostTiles.Clear();
        }

        /// <summary>
        /// Redraw the ghost piece that shows where the current piece will land in the current column.
        /// Should be called when piece rotated, column changed or tiles underneath change.
        /// </summary>
        public void RefreshGhostPiece() {
            if (!drawGhostPiece) return;

            DestroyExistingGhostTiles();

            if (piece == null) return;

            var ghostPiece = Instantiate(piece.gameObject, piece.transform.parent, true).GetComponent<Piece>();
            
            for (int i = 0; i < ghostPiece.tileCount; i++) {
                ghostPiece.GetTile(i).SetManaColor(piece.GetTile(i).manaColor, this, ghost: true);
            }

            ghostPiece.MakeGhostPiece(this, ref ghostTiles);

            // unlight all lit tiles connected by the previous ghost piece
            ForEachTile(tile => {
                tile.connectedToGhostPiece = false; tile.pulseGlow = false;
                });
            // copy state of real tiles board
            Array.Copy(tiles, 0, simulatedTiles, 0, simulatedTiles.Length);
 
            ghostPiece.PlaceTilesOnBoard(ref simulatedTiles, ghostPieceBoard);
            Destroy(ghostPiece.gameObject);

            // store all board-space positions of simulated tiles - vector2int (col, row)
            Vector2Int[] tilePositions = new Vector2Int[piece.tileCount];
            for (int i = 0; i < piece.tileCount; i++) {
                Tile tile = piece.GetTile(i);
                tilePositions[i] = piece.PieceToBoardPos(tile);
            }

            // do gravity on lower tiles before higher tiles
            Array.Sort(tilePositions, (a, b) => b.y - a.y);

            // Keep looping until none of the piece's tiles fall
            // (No other tiles need to be checked as tiles underneath them won't move, only tiles above)
            // Affect all placed tiles with gravity.
            for (int i = 0; i < tilePositions.Length; i++) {
                // pos.x = col, pos.y = row
                Vector2Int pos = tilePositions[i];
                // Only do gravity if this tile is still here and hasn't fallen to gravity yet
                if (pos.y < height && simulatedTiles[pos.y, pos.x] != null)
                {
                    // If a tile fell, set tileFell to true and the loop will go again after this
                    int newRow = GhostTileGravity(pos.y, pos.x);
                    if (newRow != pos.y)
                    {
                        tilePositions[i] = new Vector2Int(pos.x, newRow);
                    }
                }
            }

            // light all tiles connected to mana after all tiles have finished falling to reveal current blobs
            for (int i = 0; i < tilePositions.Length; i++) {
                var pos = tilePositions[i];
                LightConnectedMana(pos.y, pos.x);
            }
        }

        private struct VirtualBoardTile {
            public int row, col;
            public Tile tile;

            public VirtualBoardTile(int row, int col, Tile tile) {
                this.row = row;
                this.col = col;
                this.tile = tile;
            }
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
                int manaColor = tiles[row,col].GetManaColor();
                Color color = manaColor < 0 ? Color.white : cosmetics.manaColors[manaColor];
                SpawnParticles(row, col, color);
            }

            float pointMultiplier = tiles[row, col].pointMultiplier;

            // clear tile
            Destroy(tiles[row, col].gameObject);
            tiles[row, col] = null;

            // point multiplier should not be negative & lower damage
            return Math.Max(pointMultiplier, 0f);
        }

        /// <summary>
        /// Play the default piece-clearing partifle effect at the given row and col with specified color.
        /// </summary>
        public void SpawnParticles(int row, int col, Color color) {
            // instantiate particle system to have multiple bursts at once
            GameObject tileParticleSystem = Instantiate(clearParticleSystem, particleParent, false);

            // set particle color based on tile
            var particleSystem = tileParticleSystem.GetComponent<ParticleSystem>();
            var particleSystemMain = particleSystem.main;
            particleSystemMain.startColor = color;

            // move to tile position and play burst
            tileParticleSystem.transform.localPosition = BoardToLocalSpace(row, col);

            particleSystem.Play();
        }

        /// <summary>
        /// Make a clone of the passed particle effect object and spawn it at the given row and col.
        /// </summary>
        public void SpawnParticles(int row, int col, GameObject particleObject, Vector3 offset) {
            ParticleSystem particleSystem = Instantiate(particleObject, particleParent, true).GetComponent<ParticleSystem>();
            particleSystem.transform.localPosition = BoardToLocalSpace(row, col) + offset;
            particleSystem.Play();
        }

        public float ClearTile(int col, int row) {
            return ClearTile(col, row, true);
        }

        bool useDamageShootParticles = false;
        
        /// <summary>
        /// Deal damage / gain points. Counter incoming damage on this board first in order of closest/furthest.
        /// Residual damage is sent to the opponent, first damaging their shield and then adding to the start of their damage queue.
        /// In online mode, counter damage here first and then send residual damage to opponent to eval on their end
        /// </summary>
        /// <param name="damage">damage to deal</param>
        /// <param name="shootSpawnPos">spawn point of the damage shoot particle if using damage shoots</param>
        /// <param name="partOfChain">determines waht type of packet is sent to the opponent in online mode</param>
        public void DealDamage(int damage, Vector3 shootSpawnPos, bool partOfChain) {
            if (Storage.online) {
                // when online, only evaluate damage if the client owns this board
                if (netPlayer.isOwned) {
                    // evaluate local damage first and then send damage to opponent for them to evaluate on their client
                    int residualDamage = DealDamageLocal(damage, shootSpawnPos);
                    if (partOfChain) {
                        netPlayer.CmdAdvanceChain(startup: false, damageSent: residualDamage);
                    }
                    netPlayer.CmdUpdateDamageQueue();
                } else {
                    Debug.LogWarning("Trying to evaluate damage on a non-owned board");
                }
            // if not online, deal damage as normal
            } else {
                DealDamageLocal(damage, shootSpawnPos);
            }
        }

        /// <summary>
        /// Locally evaluate dealing damage only for this board - to be specific, either adding points to score, or countering incoming damage.
        /// </summary>
        /// <param name="damage">amount of damage to counter with / points to gain</param>
        /// <param name="shootSpawnPos">spawn position of the damage shoot (ignored if not using shoots)</param>
        /// <returns>the amount of residual damage after countering/adding shield</returns>
        public int DealDamageLocal(int damage, Vector3 shootSpawnPos)
        {
            if (postGame) {
                // just add score if postgame and singleplayer
                if (singlePlayer && !Storage.level.aiBattle)
                {
                    hp += damage;   
                }
                // otherwise if versus, nothing will happen, other player is already dead so dont damage
                return 0;
            }

            // ATTACK ANIMATIONS DISABLED (distacting & covers damange queue)
            // if (chain >= 2) attackPopup.AttackAnimation();

            if (useDamageShootParticles) {
                GameObject shootObj = Instantiate(damageShootPrefab, shootSpawnPos, Quaternion.identity, transform);
                DamageShoot shoot = shootObj.GetComponent<DamageShoot>();
                shoot.damage = damage;
                DamageShootAtTarget(shoot);
                return 0;
            } else {
                return EvaluateInstantOutgoingDamage(damage);
            }
        }

        /// <summary>
        /// Evaluate incoming damage instantly.
        /// </summary>
        /// <param name="damage">amount of damage to take</param>
        /// <returns>residual damage not dealt (only applicable to online mode where damage is sent to opponent to eval on their client)</returns>
        public int EvaluateInstantOutgoingDamage(int damage) {
            // if singleplayer, add to "score" (hp bar)
            if (singlePlayer && !Storage.level.aiBattle) {
                AddScore(damage);
                return 0;
            }

            // first try to counter incoming damage from furthest to closest
            if (damage > 0) damage = hpBar.CounterIncoming(damage);

            // then add shield to self
            if (damage > 0 && battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Shields) damage = AddShield(damage);

            // if in online mode, send remainder of damage to opponent for then to evaluate on their client
            // if not, evaluate on the other board now
            if (damage <= 0) return 0;
            if (Storage.online) {
                return damage;
            } else {
                enemyBoard.EvaluateInstantIncomingDamage(damage);
                return 0;
            }
        }

        /// <summary>
        /// Instantly evaulate receiving damage. Damages shield first and then queues damage if any leftover
        /// </summary>
        /// <param name="damage">amount of damage to take</param>
        /// <returns>amount of residual damage - the amount that </returns>
        public void EvaluateInstantIncomingDamage(int damage) {
            // Attack this board's shield first
            if (damage > 0) damage = DamageShield(damage);

            // then enqueue damage
            if (damage > 0) {
                EnqueueDamage(damage);
                PlayDamageShootSFX();
            }

            // in online, relay damage queue state to the opponent afterwards
            if (Storage.online) {
                netPlayer.CmdUpdateDamageQueue();
            }
        }

        public void PlayDamageShootSFX() {
            PlaySFX(dmgShootSFX);
        }

        public void DamageShootAtTarget(DamageShoot shoot) {
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
                        break;
                    }
                }

                // If the damage bar is empty and this fighter can make shields, make a shield if below max shield
                if (battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Shields && shield < maxShield) {
                    shoot.target = this;
                    shoot.mode = DamageShoot.Mode.Shielding;
                    shoot.destination = hpBar.shieldObject.transform.position;
                    return; 
                }

                // If this is network and not controlled by this player, skip and let other player evaluate dealt for their board.
                // they will send an rpc back after evaluation with the current damage queue state
                if (Storage.online && !netPlayer.isOwned) {
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
                if (enemyBoard.recoveryMode) {
                    Destroy(shoot.gameObject);
                   return;
                }
                
                shoot.target = enemyBoard;
                shoot.mode = DamageShoot.Mode.Attacking;
                shoot.destination = enemyBoard.hpBar.DamageQueue[0].transform.position;
            }

            if (DamageShoot.instant) {
                shoot.InstantEvaluate();
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
                // incoming damage should not damage shield, it is already "past" it in the damage queue
                TakeDamage(dmg, canDamageShield: false);
                Item.Proc(equiped, Item.DeferType.OnDamageTaken);
            }

            // advance queue
            hpBar.AdvanceDamageQueue();
        }

        /// <summary>
        /// Deal damage to this board and subtract HP.
        /// Player is defeated if HP reaches 0.
        /// </summary>
        /// <param name="damage">amount of damage to take</param>
        /// <param name="intensity">strength in which the board should be shaken</param>
        /// <param name="canDamageShield">if false, will go through shield</param>
        /// <param name="allowDeath">if false, player will not die when reaching 0, used in online to prevent desyncs</param>
        public void TakeDamage(int damage, float intensity = 1f, bool canDamageShield = false) {
            DamageShake(intensity);

            // subtract from hp
            if (canDamageShield)
            {
                int damageToShield = Math.Min(damage, shield);
                shield -= damageToShield;
                damage -= damageToShield;
            }
            hp -= damage;
            hpBar.Refresh();

            // If this player is out of HP, run defeat
            // if online and not owner, wait for other client to verify this client is dead
            bool allowDeath = !Storage.online || netPlayer.isOwned;
            if (allowDeath && hp <= 0) Defeat();

            CheckMidLevelConversations();
        }

        public void DamageShake(float intensity) {
            // shake the board and portrait when damaged
            shake.StartShake(intensity);
            portrait.GetComponent<Shake>().StartShake(intensity);
            // flash portrait red
            portrait.GetComponent<ColorFlash>().Flash(intensity);

            PlaySFX(damageTakenSFX);
        }

        public void UpdateShield() {
            hpBar.shieldNumText.text = shield+"";
            hpBar.shieldObject.SetActive( shield > 0 );
            hpBar.Refresh();
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
            public int manaColor;
            public List<Vector2Int> tiles;

            public Blob(int manaColor) {
                this.manaColor = manaColor;
                this.tiles = new List<Vector2Int>();
            }
        }


        /** Update the blob list this board has recognized. Should be called every time the board changes. */

        public void RefreshBlobs(int manaColor, float glowDelay = 0f) {
            tilesInBlobs = new bool[height, width];
            FindBlobs(manaColor);

            GlowBlobs(glowDelay);

            totalBlobMana = TotalMana(blobs);
        }

        /// <summary>Glow tiles that are currently in a blob.</summary>
        public void GlowBlobs(float glowDelay) {
            if (glowDelay > 0) {
                foreach (var blob in blobs) {
                    foreach (var tile in blob.tiles) {
                        tiles[tile.y, tile.x].AnimateGlow(1f, glowDelay);
                    }
                }
            }
        }

        /// <summary>Unglow tiles that aren't currently in any blobs.
        /// Should be called if pieces are cleared in the middle of a spellcast (via abilities).</summary>
        public void UnglowNotInBlobs() {
            for (int r=0; r<height; r++) {
                for (int c=0; c<width; c++) {
                    if (!tilesInBlobs[r, c] && tiles[r, c] != null) {
                        tiles[r, c].AnimateGlow(0f, 0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the collection of connected mana blobs.
        /// </summary>
        /// <param name="glowDelay">if above 0, start glowing mana about to be cleared</param>
        public void RefreshBlobs(float glowDelay = 0f) {
            RefreshBlobs(GetCycleColor(), glowDelay);

            // just tucked this in here, since it correlates to the lot of the same things
            // refreshes zman color obscuring
            RefreshObscuredTiles();
        }

        int chain = 0;
        int cascade = 0;

        // Check the board for blobs of 3 or more of the given color, and clear them from the board, earning points/dealing damage.
        private void Spellcast(bool refreshBlobs)
        {
            if (defeated) return;

            if (refreshBlobs) RefreshBlobs();

            // If there were no blobs, do not deal damage, and do not move forward in cycle, 
            // end spellcast if active
            if (blobs.Count == 0) {
                // Check for foresight icon, consuming it and skipping to next color if possible.
                if (abilityManager.ForesightCheck()) {
                    RefreshBlobs( GetCycleColor(1) );
                    if (totalBlobMana > 0) {
                        abilityManager.ActivateForesightSkip();
                        AdvanceCycle();
                        Spellcast(refreshBlobs: false);
                    } else {
                        RefreshBlobs( GetCycleColor() );
                        casting = false;
                        chain = 0;
                        RefreshObjectives();
                        StartCoroutine(CheckMidConvoAfterDelay());
                    }
                } else {
                    casting = false;
                    chain = 0;
                    RefreshObjectives();
                    StartCoroutine(CheckMidConvoAfterDelay());
                }
                return;
            };

            // Keep clearing while mana cleared for current color is greater than 0
            // Begin spellcast
            
            casting = true;
            if (chain == 1) {
                PlaySFX(startupCastSFX);
                if (Storage.online && netPlayer.isOwned) netPlayer.CmdAdvanceChain(startup: true, 0);
            }

            GlowBlobs(chainDelay);
            if (manualCastContinue) StartCoroutine(AdvanceChainAndCascadeAfterDelay(chainDelay));
        }

        // Called in online mode to indicate when an opponent is starting their spellcast
        public void StartupEffect() {
            GlowBlobs(chainDelay);
            PlaySFX(startupCastSFX);
        }

        private static readonly float cascadeDelay = 0.6f, chainDelay = 0.8f;
        IEnumerator AdvanceChainAndCascadeAfterDelay(float delay)
        {
            // Brief delay before clearing current color
            yield return new WaitForSeconds(delay);
            AdvanceChainAndCascade();
        }

        public void AdvanceChainAndCascade() {
            if (defeated) return;

            // Check for any new blobs that may have formed in the delay before this was called.
            // (Replaces old list)
            // update: called on every place, so no longer needed here
            // RefreshBlobs(color);

            if (chain > 1) chainPopup.Flash(chain.ToString());

            // advance cascade count
            // if 2 or higher will show a notification
            cascade += 1;
            if (cascade > 1)
            {
                PlaySFX(cascadeSFX, 1f + cascade * 0.1f);
                cascadePopup.Flash(cascade.ToString());
            }

            // Get the average of all tile positions; this is where shoot particle is spawned
            Vector3 averagePos = Vector3.zero;
            foreach (Blob blob in blobs) {
                for (int index = 0; index < blob.tiles.Count; index++) {
                    var pos = blob.tiles[index];
                    if (tiles[pos.y, pos.x] == null) {
                        Debug.LogWarning("Trying to clear null tile found in blob: "+pos.y+", "+pos.x);
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
            abilityManager.GainMana((int) (totalBlobMana * boardStats[SpecialGainMult]));

            highestCombo = Math.Max(highestCombo, chain);
            highestCascade = Math.Max(highestCascade, cascade);

            float totalPointMult = 0;
            // Clear all blob-contained tiles from the board.
            foreach (Blob blob in blobs) {
                // run onclear first to check for point multiplier increases or other effects (geo gold mine)
                foreach (Vector2Int pos in blob.tiles) {
                    if (tiles[pos.y, pos.x]) {
                        tiles[pos.y, pos.x].BeforeClear(blob);
                    }
                }
                // then remove the tiles from the board 
                foreach (Vector2Int pos in blob.tiles) {
                    totalPointMult += ClearTile(pos.x, pos.y);

                    // clear adjacent fragile tiles (mini z?men)
                    totalPointMult += ClearTile(pos.x - 1, pos.y, true, onlyClearFragile: true);
                    totalPointMult += ClearTile(pos.x + 1, pos.y, true, onlyClearFragile: true);
                    totalPointMult += ClearTile(pos.x, pos.y - 1, true, onlyClearFragile: true);
                    totalPointMult += ClearTile(pos.x, pos.y + 1, true, onlyClearFragile: true);
                }
            }
            if (cascade <= 1) PlaySFX(castSFX, 1f + chain * 0.1f);

            // Geo's revenge system
            float GeoBoost = 1f;
            if(battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.LastStand){
                if(hp <= (float)maxHp/4){
                    GeoBoost = 1.4f;
                    Debug.Log("Geoboost 25% ver!!!");
                }
                else if(hp <= (float)maxHp/2){
                    GeoBoost = 1.15f;
                    Debug.Log("Geoboost 50% ver!!!");
                }
            }

            // Deal damage for the amount of mana cleared.
            // DMG is scaled by chain and cascade.
            int damage = (int)( totalPointMult * damagePerMana * chain * (Math.Pow(3,cascade) / 3f) * boardStats[DamageMult] *GeoBoost);

            highestSingleDamage = Math.Max(highestSingleDamage, damage);

            // relay to the opponent's client that the chain advance happened at this point in time
            // RPCs should be guaranteed to execute in order send, so desyncs where piece is placed after when it should in the chain should be prevented.
            // but if stuff starts breaking it probably has to do with RPC execution order, im not 100% sure its guaranteed.

            // Send the damage over. Will counter incoming damage first.
            // when in onlinemode, residual damage may be left that will be sent to the opponent to add to their damage queue within their client
            // and they will then send back an rpc with their damage queue state
            // do not deal damage here if online;
            // instead the damageSent part of the AdvanceChain rpc will be used to call damage after this was calle dto update the boar dand clear the mana
            DealDamage(damage, averagePos, partOfChain: true);

            // Do gravity everywhere
            AllTileGravity();

            // Check for cascaded blobs
            RefreshBlobs();

            RefreshGhostPiece();
            RefreshObjectives();
            StartCoroutine(CheckMidConvoAfterDelay());

            // cascade loop will continue if there are any mana blob tiles of the current color available to clear
            if (totalBlobMana > 0) {
                GlowBlobs(cascadeDelay);
                if (manualCastContinue) StartCoroutine(AdvanceChainAndCascadeAfterDelay(cascadeDelay));
            } else {
                AdvanceCycle();
                cascade = 0;
                chain++;
                Spellcast(refreshBlobs: true);
            }
        }

        private void AdvanceCycle() {
            // move to next in cycle position
            cyclePosition += 1;
            if (cyclePosition >= ManaCycle.cycleLength) {
                cyclePosition = 0;
                cycleLevel++;
                cycleLevelDisplay.Set(cycleLevel);
                Item.Proc(equiped, Item.DeferType.OnFullCycle);
                PlaySFX(fullCycleSFX);
            }
            PointerReposition();

            UpdateCycleColoredObjects();
        }


        private static Color fadeToColor = new Color(0.95f, 0.97f, 1f);
        // update image color of cycle colored objects
        private void UpdateCycleColoredObjects()
        {
            foreach (ColorFader cycleColoredObject in cycleColoredObjects)
            {
                Color cycleColor = cosmetics.GetVisualManaColor(cycle.GetCycle()[cyclePosition]);
                Color brightenedColor = Color.Lerp(cycleColor, fadeToColor, 0.3f);
                cycleColoredObject.FadeToColor(brightenedColor);
            }
        }

        public int damagePerMana {get {return 10 + (int)((cycleLevel + boardStats[StartingCycleModifier]) * boostPerCycleClear);}}

        IEnumerator CheckMidConvoAfterDelay() {
            yield return new WaitForSeconds(0.4f);
            CheckMidLevelConversations();
        }

        // Updates list of all blobs of mana that were cleared.
        private void FindBlobs(int manaColor)
        {
            blobs.Clear();

            // Loop over rows (top to bottom)
            for (int r = 0; r < height; r++)
            {
                // Loop over columns (left to right)
                for (int c = 0; c < width; c++)
                {
                    // Check if indexed tile exists and is correct color, and is not in a blob
                    if (tiles[r, c] != null && tiles[r, c].GetManaColor() == manaColor && !tilesInBlobs[r, c])
                    {
                        Blob blob = CheckBlob(c, r, manaColor);
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
        Blob CheckBlob(int c, int r, int manaColor)
        {
            Blob blob = new Blob(manaColor);

            ExpandBlob(ref blob, c, r, manaColor);

            return blob;
        }

        // Recursively expands the passed blob to all connected tiles
        void ExpandBlob(ref Blob blob, int c, int r, int manaColor)
        {
            // Don't add to blob if the tile is in an invalid position
            if (c < 0 || c >= width || r < 0 || r >= height) return;

            // Don't add to blob if already in this blob or another blob; this would cause an infinite loop
            if (tilesInBlobs[r, c]) return;

            // Don't add if there is not a tile here
            if (tiles[r, c] == null) return;

            // Don't add if the tile is the incorrect color & this or target tile is not set to Any (multicolor) (geo crystal)
            if (tiles[r, c].GetManaColor() != manaColor 
            && manaColor != ManaColor.Any
            && tiles[r, c].GetManaColor() != ManaColor.Any) return;

            // Add the tile to the blob and fill in its spot on the tilesInBlobs matrix
            blob.tiles.Add(new Vector2Int(c, r));
            tilesInBlobs[r, c] = true;

            // Expand out the current blob on all sides, checking for the same colored tile to add to this blob
            ExpandBlob(ref blob, c-1, r, manaColor);
            ExpandBlob(ref blob, c+1, r, manaColor);
            ExpandBlob(ref blob, c, r-1, manaColor);
            ExpandBlob(ref blob, c, r+1, manaColor);
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
                    // this is the row that the tile fell to (fr) (for real) (frfr)
                    int fr = rFall-1;
                    // only fall if tile is in a different position than before
                    if (fr != r) {
                        tiles[fr, c] = tiles[r, c];
                        // I am subtracting half of width and height again here, because it only works tht way,
                        // i don't know enough about transforms to know why. bandaid solution moment.
                        tiles[fr, c].transform.localPosition = BoardToLocalSpace(fr, c);

                        // Animate falling from offset from current row down to current row.
                        tiles[fr, c].AnimateMovement(
                            new Vector2(0, fr - r),
                            new Vector2(0, 0)
                        );

                        // clear old reference to this tile from old position
                        tiles[r, c] = null;
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// Translate a row and col position into the correct local position on the pieceBoard.transform.
        /// </summary>
        public Vector3 BoardToLocalSpace(int row, int col) {
            return new Vector3(
                col - width/2f + 0.5f, 
                -row + physicalHeight/2f - 0.5f + height - physicalHeight, 
                0
            );
        }

        /// <returns>the row that the piece fell to (or stayed at)</returns>
        public int GhostTileGravity(int r, int c)
        {
            // If there isn't a tile here, it can't fall, because it isnt a tile...
            // OR, If the tile is an antigravity tile, do not pull it down. (etc. infinity's sword)
            // only pull downward if forced, e.g. by Infinity's ability itslef pulling it down
            // as opposed to from an AllTileGravity from a clear
            if (simulatedTiles[r, c] == null || !simulatedTiles[r, c].doGravity) {
                Debug.LogWarning("no simulated tile at r="+r+", c="+c);
                return r;
            }

            // For each tile, check down until there is no longer an empty tile
            for (int rFall = r+1; rFall <= height; rFall++)
            {
                // Once a non-empty is found, or reached the bottom move the tile to right above it
                if (rFall == height || simulatedTiles[rFall, c] != null)
                {
                    // only fall if tile is in a different position than before
                    if (rFall-1 != r) {
                        simulatedTiles[rFall-1, c] = simulatedTiles[r, c];

                        // ddo not animate tile; have it immediately snap to where it will fall
                        simulatedTiles[rFall-1, c].transform.localPosition = new Vector3(
                            c - width/2f + 0.5f, 
                            -rFall + 1 + physicalHeight/2f - 0.5f + height - physicalHeight, 
                            0.5f // move tile away from camera so it is rendered behind non-ghost (real) tiles
                        );

                        simulatedTiles[r, c] = null;
                        return rFall-1;
                    }
                }
            }

            return r;
        }


        // This and ExpandLightBlob are mostly analogous to CheckBlob() and ExpandBlob(), except for lighting tiles & slight differences
        void LightConnectedMana(int row, int col) {
            int manaColor = simulatedTiles[row, col].manaColor;
            Blob blob = new Blob(manaColor);

            ExpandLightBlob(row, col, manaColor, ref blob);

            // ExpandLightBlob(row-1, col, color, ref blob);
            // ExpandLightBlob(row+1, col, color, ref blob);
            // ExpandLightBlob(row, col-1, color, ref blob);
            // ExpandLightBlob(row, col+1, color, ref blob);

            // if blob is big enough, light up all connected mana
            if (blob.tiles.Count >= minBlobSize) {
                blob.tiles.ForEach(tile => simulatedTiles[tile.x, tile.y].pulseGlow = true);
            } 

            // otherwise, disconnect it; a later tile might be able to use this in a big enough blob
            else {
                simulatedTiles[row, col].connectedToGhostPiece = false;
            }
        }

        void ExpandLightBlob(int row, int col, int manaColor, ref Blob blob) {
            // return if OOB
            if (row < 0 || row >= height || col < 0 || col >= width) return;
            // return if no sim. tile
            if (!simulatedTiles[row, col]) return;
            // return if incorrect color
            if (simulatedTiles[row, col].manaColor != manaColor) return;
            // return if already connected - covers condition of already being in a blob
            if (simulatedTiles[row, col].connectedToGhostPiece) return;
            // cannot see the color of obscured tiles
            if (simulatedTiles[row, col].obscured) return;

            // Because simulatedTiles copies references to Tile objs on tiles arr, this will affect the correct tile on the real tiles board
            simulatedTiles[row, col].connectedToGhostPiece = true;
            blob.tiles.Add(new Vector2Int(row, col));

            // spread to adjacent tiles
            ExpandLightBlob(row-1, col, manaColor, ref blob);
            ExpandLightBlob(row+1, col, manaColor, ref blob);
            ExpandLightBlob(row, col-1, manaColor, ref blob);
            ExpandLightBlob(row, col+1, manaColor, ref blob);
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
        public bool CheckColor(int r, int c, int manaColor)
        {
            // return false if there is no tile at the index
            if (tiles[r,c] == null) return false;
            // if there is a tile, return true if it is the given color.
            return tiles[r,c].GetManaColor() == manaColor;
        }

        public int GetCycleColor(int offset) {
            return cycle.GetColor( (cyclePosition+offset) % ManaCycle.cycleLength );
        }

        public int GetCycleColor()
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



        // Called when defeated. Lose one life and update life transform
        // If more than one life remains, clears the board and incoming damage, and player suffers a 5-second delay.
        void LoseLife() {
            lives--;
            Destroy(lifeTransform.GetChild(0).gameObject);

            if (lives > 0) {
                PlaySFX(loseSFX);

                recoveryMode = true;
                recoveryTimer = recoveryDelay;

                boardBackground.color = boardRecoverColor;
                recoveryText.enabled = true;

                for (int r=0; r<height; r++) {
                    for (int c=0; c<width; c++) {
                        ClearTile(c, r);
                    }
                }
                for (int i=0; i<6; i++) {
                    hpBar.DamageQueue[i].SetDamage(0);
                }

                hpBar.Refresh();
            }
        }

        public void Pause() {
            // No pausing in online mode
            // for now
            // but will probably stay that way until it becomes a problem if it ever does
            if (Storage.online) return;

            if (convoPaused) {
                convoHandler.EndConvo();
                return;
            }

            if (!postGame && !Storage.convoEndedThisInput && pauseMenu.gameObject.activeInHierarchy)
            {
                pauseMenu.TogglePause();
                Instantiate(PauseSFX);
            }
        }

        public void Defeat() 
        {
            recoveryMode = false;
            if (defeated) {
                Debug.LogWarning("Trying to defeat while already defeated");
                return;
            }
            if (won) {
                Debug.LogWarning("Trying to defeat while already won");
                return;
            }

            piece.DestroyTiles();
            Destroy(piece.gameObject);
            DestroyExistingGhostTiles();
            hpBar.hpNum.gameObject.SetActive(false);
            defeated = true;
            casting = false;

            LoseLife();

            if (lives > 0) return;

            postGame = true;
            if (timer != null) timer.StopTimer();
            foreach (var incoming in hpBar.DamageQueue) {
                incoming.SetDamage(0);
            }

            // pieceBoard.SetActive(false);
            winText.gameObject.SetActive(true);
            winText.text = "LOSE";

            boardDefeatFall.StartFall();

            if (!singlePlayer) enemyBoard.Win();

            if (level != null) {
                winMenu.AppearAfterDelay();
                Instantiate(loseSFX);
                SoundManager.Instance.PauseBGM();
            }

            StartCoroutine(CheckMidConvoAfterDelay());
        }

        public void Win()
        {
            recoveryMode = false;
            if (postGame || won) return;

            //Item.Proc(equiped, Item.DeferType.PostGame);

            postGame = true;
            won = true;
            if (timer != null) timer.StopTimer();
            foreach (var incoming in hpBar.DamageQueue) {
                incoming.SetDamage(0);
            }

            winText.gameObject.SetActive(true);
            winText.text = "WIN";

            winMenu.AppearAfterDelay();

            StartCoroutine(CheckMidConvoAfterDelay());

            SoundManager.Instance.PauseBGM();
            Instantiate(winSFX);
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

        public bool IsBattleStarted()
        {
            return battleStarted;
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

        public int GetHighestCascade() {
            return highestCascade;
        }

        public int GetManualSpellcasts() {
            return totalManualSpellcasts;
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

        public void Heal(int amount)
        {
            hp = Math.Min(hp, maxHp + amount);
        }

        public void SetHp(int amount, bool allowDeath = true) {
            hp = amount;
            hpBar.Refresh();
            if (allowDeath && hp <= 0) Defeat();
        }

        /// <summary>
        /// play a sound from this board.
        /// </summary>
        public void PlaySFX(GameObject sfxObject, float pitch = 1f)
        {
            SFXObject sfx = Instantiate(sfxObject).GetComponent<SFXObject>();
            if(!singlePlayer) sfx.pan = 0.3f * (playerSide * 2 - 1);
            sfx.pitch = pitch;
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

        // username / local name
        public void SetUsername(string name) {
            usernameLabel.gameObject.SetActive(true);
            usernameLabel.text = name;
        }

        public void HideUsername() {
            usernameLabel.gameObject.SetActive(false);
        }
    }
}