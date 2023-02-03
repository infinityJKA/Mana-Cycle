using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public ManaColor color { get; private set; }
    public ManaCycle cycle;

    public void Randomize(GameBoard board)
    {
        // Randomly choose color from color enum length
        // Color has a 15% chance to boe current color, and 85% chance to be random color (including current).
        // color = (Random.value < 0.2) ? board.CurrentColor() : (ManaColor)Random.Range(0,5); <-- will be infinity's rng pattern
        color = (ManaColor)Random.Range(0,5);

        // Get image and set color from the list in this scene's cycle
        Image image = GetComponent<Image>();
        image.color = board.cycle.GetManaColors()[ ((int)color) ];
    }

    public ManaColor GetManaColor()
    {
        return color;
    }

    public void SetColor(ManaColor color)
    {
        this.color = color;
    }
}
