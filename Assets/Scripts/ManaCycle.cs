using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ManaCycle : MonoBehaviour
{
    // Prefab for cycle colors to display
    [SerializeField] private Image cycleColorPrefab;
    // All ManaColor colors to tint the Tile images
    [SerializeField] private List<Color> manaColors;

    // List of all colors in the cycle
    private List<ManaColor> cycle;
    // List of all cycleColor objects that represent the colors
    private List<Image> cycleObjects;
    // Length of the cycle
    private int cycleLength = 7;
    void Start()
    {
        GenerateCycle();
    }

    public void GenerateCycle()
    {
        // Destroy all children (violently, and without remorse)
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }

        // Initialize a new list (will override old ones)
        cycle = new List<ManaColor>();
        // Start color at -1, or no color, so color first can be any color
        int color = -1;
        for (int i=0; i<cycleLength; i++) {
            int newColor = color;
            // Set color to random until it is different than the last color
            while (newColor == color)
            {
                newColor = Random.Range(0,5);
            }

            Debug.Log(color);
            color = newColor;
            cycle.Add((ManaColor) color);

            // Create cycle color object and add to this object as a child
            Image cycleObject = Instantiate(cycleColorPrefab, Vector3.zero, Quaternion.identity);
            cycleObject.color = manaColors[color];
            cycleObjects.Add(cycleObject);
            cycleObject.transform.SetParent(transform);
        }
    }

    public List<Color> GetManaColors()
    {
        return manaColors;
    }
}