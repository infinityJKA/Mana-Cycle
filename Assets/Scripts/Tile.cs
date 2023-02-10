using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public ManaColor color { get; private set; }
    public ManaCycle cycle;

    public void SetColor(ManaColor color, GameBoard board)
    {
        // Get image and set color from the list in this scene's cycle
        this.color = color;
        Image image = GetComponent<Image>();
        image.color = board.cycle.GetManaColors()[ ((int)color) ];
    }

    public ManaColor GetManaColor()
    {
        return color;
    }
}
