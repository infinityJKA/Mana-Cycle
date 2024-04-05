using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Battle.Board;
using Mirror;

namespace Battle.Cycle {
    public class ManaCycle : MonoBehaviour
    {
        public static ManaCycle instance;

        // Whether or not ManaCycle has been properly loaded in yet.
        // Set to false when Replay() or match is started and set to true after Start() is finished running here.
        // Ensures proper logic flow during online rematches
        public static bool initializeFinished = false;

        // Prefab for cycle colors to display
        [SerializeField] private GameObject manaImageObject;
        [SerializeField] private Image bgImage;

        // All GameBoards in the scene that use this cycle
        [SerializeField] private List<GameBoard> boards;
        public List<GameBoard> Boards => boards;

        [SerializeField] private CountdownHandler countdownHandler;
        public CountdownHandler CountdownHandler => countdownHandler;

        // List of all colors in the cycle
        public static int[] cycle;

        // List of all cycleColor objects that represent the colors - generated on scene start
        private List<TileVisual> cycleObjects;

        // Length of the cycle
        public static int cycleLength = 7;
        // Amount of unique colors in the cycle
        public static int cycleUniqueColors = 5;
        // whether or not the peices you can pull are locked to cycle colors. true in most cases, false in some levels
        public static bool lockPieceColors = true;

        public bool usingSprites;

        private void Awake() {
            instance = this;
        }

        private void Start() {
            // generate the cycle for this battle
            // do not generate if this is online, setup cmd will initialize and sync cycles
            if (!Storage.online) GenerateCycle();

            // Connect players to boards
            if (Storage.online) {
                var players = FindObjectsByType<NetPlayer>(FindObjectsSortMode.None);
                Debug.Log("players found: "+players.Length);
                foreach (var player in players) {
                    player.board = boards[player.isOwned ? 0 : 1];
                    player.board.SetNetPlayer(player);
                    // reset rematchRequested to false incase scene is being reloaded during a rematch match
                    player.board.netPlayer.postGameIntention = NetPlayer.PostGameIntention.Undecided;
                }

                foreach (var player in players) {
                    // on host side, call command that will initialize rng & other stuff and send to other client
                    if (NetworkServer.activeHost && player.isLocalPlayer) {
                        player.CmdBattleInit();
                    }
                }
            }

            // when not in online mode, just pick a random seed for both rngs
            else {
                foreach (var board in boards) {
                    board.rngManager.SetSeed(NetPlayer.seedGenerator.Next());
                }
            }

            // creates the cycle and displays it on the screen.
            // don't run here if online; postpone this in online wait until data is received from the host
            if (!Storage.online) CreateCycleObjects();
            
            ManaCycle.initializeFinished = true;

            foreach (var board in boards) {
                // if any netPlayer is waiting on scene load and this cycle for initialization,
                // this will call that delayed init
                if (Storage.online) {
                    if (board.netPlayer) {
                        board.netPlayer.OnBattleSceneLoaded();
                    } else {
                        Debug.LogWarning(board+" is missing a NetPlayer while in online mode");
                    }
                }
            }

        }

        /// <summary>
        /// Initialize boards when the countdown hits 0
        /// </summary>
        public void StartBattle()
        {
            // Start game boards - their first piece will begin falling.
            foreach (GameBoard board in boards)
            {
                if (board.enabled) board.StartBattle();
                if (!board.gameObject.activeInHierarchy) board.pointer.SetActive(false);
            }
        }

        /// <summary>
        /// Is run near the end of start method, before boards are initialized with cycle. Creates the cycle objects and displays them on the screen.
        /// Also runs InitializeWithCycle() on all boards./// 
        /// </summary>
        public void CreateCycleObjects() {
            // Display the icons and colors for player 1's board.
            // BoardCosmeticAssets cosmetics = boards[0].cosmetics;

            // Check if player 1 is in single player. if so, use its cycle length variables
            if (Storage.level) {
                boards[1].pointer.SetActive(false);
                boards[1].pointer.gameObject.SetActive(false);
            } else {
                cycleLength = 7;
                cycleUniqueColors = 5;
            }

            // Destroy all children (violently, and without remorse)
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }

            cycleObjects = new List<TileVisual>();

            // Create cycle color objects for each cycle color
            for (int i=0; i<cycleLength; i++)
            {
                TileVisual cycleObject = Instantiate(manaImageObject, Vector3.zero, Quaternion.identity).GetComponent<TileVisual>();
                // cycleObject.color = cosmetics.manaColors[cycle[i]].mainColor;
                cycleObject.SetVisual(boards[0], cycle[i]);
                cycleObject.DisableVisualUpdates();
                cycleObjects.Add(cycleObject);
                cycleObject.transform.SetParent(transform, false);
            }

            foreach (var board in boards) {
                // Setup cycle and many other components on each board (tile grid, etc)
                board.InitializeWithCycle(this);
            }
        }

        public static void GenerateCycle()
        {
            if (Storage.level) {
                Debug.Log("cycle len: "+Storage.level.cycleLength);
                cycleLength = Storage.level.cycleLength;
                cycleUniqueColors = Storage.level.cycleUniqueColors;
                lockPieceColors = Storage.level.lockPieceColors;
            } else {
                cycleLength = 7;
                cycleUniqueColors = 5;
            }

            cycle = new int[cycleLength];

            // Add one of each color to the list
            for (int i=0; i<cycleUniqueColors; i++)
            {
                cycle[i] = i;
            }

            // Add random colors until length is met
            for (int i=cycleUniqueColors; i<cycleLength; i++)
            {
                cycle[i] = Random.Range(0, cycleUniqueColors);
            }

            // Shuffle the list
            Utils.Shuffle(cycle);

            // For each color, check that the color below is not the same color
            for (int i=0; i<cycleLength-1; i++)
            {
                // If it is, swap the color to a random color that is not either of the colors next to it
                // If at the top, tile above is the tile at the bottom, which is the one before it
                int colorAbove = (i == 0) ? cycle[cycle.Length-1] : cycle[i-1];
                int colorBelow = cycle[i+1];

                // Keep picking a new color until it is different than the one above & below
                // don't run if cycle length and unique color amount make this impossible
                while ((cycle[i] == colorAbove || cycle[i] == colorBelow) && (cycleUniqueColors != 1))
                {
                    cycle[i] = Random.Range(0,cycleUniqueColors);
                }
            }
        }

        public static void SetCycle(int[] cycle) {
            ManaCycle.cycle = cycle;
        }

        public int[] GetCycle()
        {
            return cycle;
        }

        public int GetColor(int index)
        {
            return cycle[index];
        }
    }
}