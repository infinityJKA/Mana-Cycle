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
    private int cycleLength = 7;
    void Start()
    {
        // Generate cycle that boards will use
        GenerateCycle();

        // Initialize game boards
        foreach (GameBoard board in boards)
        {
            board.InitializeCycle(this);
        }
    }

    private int checkCountInArray(int[] ar, int c){
        if(ar.Length < 1){
            return 0;
        }
        int no = 0;
        for(int i = 0; i < ar.Length-1; i++){
            if(ar[i] == c){
                no++;
            }
        }
        return no;
    }

    public void GenerateCycle()
    {
        // Destroy all children (violently, and without remorse)
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
        cycleObjects = new List<Image>();

        // Initialize a new list (will override old ones)
        cycle = new List<ManaColor>();
        // Start color at -1, or no color, so color first can be any color
        int color = -1;
        int[] colors = new int[cycleLength];
        for (int i=0; i<cycleLength; i++) {
            int newColor = color;
            // Set color to random until it is different than the last color
            while (newColor == color)
            {
                newColor = Random.Range(0,5);
                if (checkCountInArray(colors,newColor) >= 2){
                    newColor = color;
                }
            }

            Debug.Log(color);
            color = newColor;
            cycle.Add((ManaColor) color);

            // Create cycle color object and add to this object as a child
            Image cycleObject = Instantiate(cycleColorPrefab, Vector3.zero, Quaternion.identity);
            cycleObject.color = manaColors[color];
            cycleObjects.Add(cycleObject);
            cycleObject.transform.SetParent(transform, false);
        }
    }

    public List<ManaColor> GetCycle()
    {
        return cycle;
    }

    public List<Color> GetManaColors()
    {
        return manaColors;
    }
}