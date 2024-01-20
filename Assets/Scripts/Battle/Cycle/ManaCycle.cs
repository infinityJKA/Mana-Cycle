using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Battle.Board;
using Mirror;

namespace Battle.Cycle {
    public class ManaCycle : MonoBehaviour
    {
        public static ManaCycle instance;

        // Prefab for cycle colors to display
        [SerializeField] private Image manaImage;
        [SerializeField] private Image bgImage;

        // All ManaColor colors to tint the cycle images
        [SerializeField] private List<Color> manaColors;

        // Color to light mana tiles when they are shown as connected to the ghost piece.
        [SerializeField] private List<Color> litManaColors;

        // String representations of the mana color
        [SerializeField] public List<string> manaColorStrings;

        // List of sprites to use for mana of this color (corresponds to indexes in manaColors)
        [SerializeField] public List<Sprite> manaSprites;

        // List of sprites for ghost piece tiles.
        [SerializeField] public List<Sprite> ghostManaSprites;

        // Used for Geo's gold mine crystals that correspond to colors in this cycle.
        [SerializeField] public List<Material> crystalMaterials;

        // All GameBoards in the scene that use this cycle
        [SerializeField] private List<GameBoard> boards;
        public List<GameBoard> Boards => boards;

        // List of all colors in the cycle
        public static ManaColor[] cycle;

        // List of all cycleColor objects that represent the colors
        private List<Image> cycleObjects;

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
                Debug.LogWarning("players found: "+players.Length);
                foreach (var player in players) {
                    player.board = boards[player.isOwned ? 0 : 1];
                    player.board.netPlayer = player;

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
        }

        /// <summary>
        /// Initialize boards when the countdown hits 0
        /// </summary>
        public void InitBoards()
        {
            // Check if player 1 is in single player. if so, use its cycle length variables
            if (Storage.level) {
                cycleLength = Storage.level.cycleLength;
                cycleUniqueColors = Storage.level.cycleUniqueColors;
                lockPieceColors = Storage.level.lockPieceColors;
                boards[1].pointer.SetActive(false);
                boards[1].pointer.gameObject.SetActive(false);
            } else {
                cycleLength = 7;
                cycleUniqueColors = 5;
            }

            CreateCycleColorObjects();

            // Initialize game boards
            foreach (GameBoard board in boards)
            {
                if (board.enabled) board.InitializeCycle(this);
                if (!board.enabled) board.pointer.SetActive(false);
            }
        }

        public static void GenerateCycle()
        {
            cycle = new ManaColor[cycleLength];

            // Add one of each color to the list
            for (int i=0; i<cycleUniqueColors; i++)
            {
                cycle[i] = (ManaColor)i;
            }

            // Add random colors until length is met
            for (int i=cycleUniqueColors; i<cycleLength; i++)
            {
                cycle[i] = (ManaColor)Random.Range(0,cycleUniqueColors);
            }

            // Shuffle the list
            Utils.Shuffle(cycle);

            // For each color, check that the color below is not the same color
            for (int i=0; i<cycleLength-1; i++)
            {
                // If it is, swap the color to a random color that is not either of the colors next to it
                // If at the top, tile above is the tile at the bottom, which is the one before it
                ManaColor colorAbove = (i == 0) ? cycle[cycle.Length-1] : cycle[i-1];
                ManaColor colorBelow = cycle[i+1];

                // Keep picking a new color until it is different than the one above & below
                // don't run if cycle length and unique color amount make this impossible
                while ((cycle[i] == colorAbove || cycle[i] == colorBelow) && (cycleUniqueColors != 1))
                {
                    cycle[i] = (ManaColor)Random.Range(0,cycleUniqueColors);
                }
            }
        }

        public static void SetCycle(ManaColor[] cycle) {
            ManaCycle.cycle = cycle;
        }

        public void CreateCycleColorObjects() {
            // Destroy all children (violently, and without remorse)
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }

            cycleObjects = new List<Image>();

            // Create cycle color objects for each cycle color
            for (int i=0; i<cycleLength; i++)
            {
                Image cycleObject = Instantiate(manaImage, Vector3.zero, Quaternion.identity);
                // Image bgObject = Instantiate(bgImage, Vector3.zero, Quaternion.identity);
                cycleObject.color = manaColors[(int)cycle[i]];
                // bgObject.color = cycleObject.color = manaColors[(int)cycle[i]];
                if (usingSprites) cycleObject.sprite = manaSprites[(int)cycle[i]];
                cycleObjects.Add(cycleObject);
                // cycleObjects.Add(bgObject);
                cycleObject.transform.SetParent(transform, false);
                // bgObject.transform.SetParent(transform, false);
            }
        }

        public ManaColor[] GetCycle()
        {
            return cycle;
        }

        public ManaColor GetColor(int index)
        {
            return cycle[index];
        }

        public List<Color> GetManaColors()
        {
            return manaColors;
        }

        public Color GetManaColor(int index) {
            return manaColors[index];
        }

        public Color GetLitManaColor(int index) {
            return litManaColors[index];
        }
    }
}