using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ManaCycle : MonoBehaviour
{
    // Prefab for cycle colors to display
    [SerializeField] private Image cycleColorPrefab;

    // All ManaColor colors to tint the Tile images
    [SerializeField] private List<Color> manaColors;

    // All GameBoards in the scene that use this cycle
    [SerializeField] private List<GameBoard> boards;

    // List of all colors in the cycle
    private List<ManaColor> cycle;
    // List of all cycleColor objects that represent the colors
    private List<Image> cycleObjects;
    // Length of the cycle
    public static int cycleLength = 7;


    void Start()
    {
        // Generate cycle that boards will use
        GenerateCycle();

    }

    public void InitBoards()
    {
        // Initialize game boards
        foreach (GameBoard board in boards)
        {
            board.InitializeCycle(this);
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
        for (int i=0; i<5; i++)
        {
            cycle.Add((ManaColor)i);
        }

        // Add two more random colors
        for (int i=0; i<2; i++)
        {
            cycle.Add((ManaColor)Random.Range(0,5));
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
                cycle[i] = (ManaColor)Random.Range(0,5);
            }
        }

        // Create cycle color objects for each cycle color
        for (int i=0; i<cycleLength; i++)
        {
            Image cycleObject = Instantiate(cycleColorPrefab, Vector3.zero, Quaternion.identity);
            cycleObject.color = manaColors[(int)cycle[i]];
            cycleObjects.Add(cycleObject);
            cycleObject.transform.SetParent(transform, false);
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
}