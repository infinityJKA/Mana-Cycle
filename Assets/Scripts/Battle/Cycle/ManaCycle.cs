using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Battle.Board;

namespace Battle.Cycle {
    public class ManaCycle : MonoBehaviour
    {
        // Prefab for cycle colors to display
        [SerializeField] private Image manaImage;
        [SerializeField] private Image bgImage;

        // All ManaColor colors to tint the cycle images
        [SerializeField] private List<Color> manaColors;

        // String representations of the mana color
        [SerializeField] public List<string> manaColorStrings;

        // List of sprites to use for mana of this color (corresponds to indexes in manaColors)
        [SerializeField] public List<Sprite> manaSprites;

        // All GameBoards in the scene that use this cycle
        [SerializeField] private List<GameBoard> boards;

        // List of all colors in the cycle
        private List<ManaColor> cycle;

        // List of all cycleColor objects that represent the colors
        private List<Image> cycleObjects;

        // Length of the cycle
        public static int cycleLength = 7;
        // Amount of unique colors in the cycle
        public static int cycleUniqueColors = 5;

        public bool usingSprites;

        public void InitBoards()
        {
            // Check if player 1 is in single player. if so, use its cycle length variables
            if (boards[0].singlePlayer) {
                cycleLength = boards[0].GetLevel().cycleLength;
                cycleUniqueColors = boards[0].GetLevel().cycleUniqueColors;
                boards[1].pointer.SetActive(false);
                boards[1].pointer.gameObject.SetActive(false);
            } else {
                cycleLength = 7;
                cycleUniqueColors = 5;
            }

            GenerateCycle();

            // Initialize game boards
            foreach (GameBoard board in boards)
            {
                if (board.enabled) board.InitializeCycle(this);
                if (!board.enabled) board.pointer.SetActive(false);
            }
        }

        public void GenerateCycle()
        {
            // Destroy all children (violently, and without remorse)
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }
            cycleObjects = new List<Image>();
            cycle = new List<ManaColor>();

            // Add one of each color to the list
            for (int i=0; i<cycleUniqueColors; i++)
            {
                cycle.Add((ManaColor)i);
            }

            // Add random colors until length is met
            for (int i=cycleUniqueColors; i<cycleLength; i++)
            {
                cycle.Add((ManaColor)Random.Range(0,cycleUniqueColors));
            }

            // Shuffle the list
            Utils.Shuffle(cycle);

            // For each color, check that the color below is not the same color
            for (int i=0; i<cycleLength-1; i++)
            {
                // If it is, swap the color to a random color that is not either of the colors next to it
                // If at the top, tile above is the tile at the bottom, which is the one before it
                ManaColor colorAbove = (i == 0) ? cycle[cycle.Count-1] : cycle[i-1];
                ManaColor colorBelow = cycle[i+1];

                // Keep picking a new color until it is different than the one above & below
                while (cycle[i] == colorAbove || cycle[i] == colorBelow)
                {
                    cycle[i] = (ManaColor)Random.Range(0,cycleUniqueColors);
                }
            }

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

        public List<ManaColor> GetCycle()
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
    }
}