using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    // Color of this title. (Public read but no public write)
    private ManaColor color;

    private ManaCycle cycle;

    // Start is called before the first frame update
    void Start()
    {
        cycle = GameObject.Find("Cycle").GetComponent<ManaCycle>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ManaColor GetManaColor()
    {
        return color;
    }

    public void Randomize(GameBoard board)
    {
        // Randomly choose color from color enum length
        color = Random.value < 0.2f ? cycle.GetColor(board.cyclePosition) : (ManaColor) Random.Range(0,5);

        // Get image and set color from the list in this scene's cycle
        GetComponent<Image>().color = cycle.GetManaColors()[((int)color)];
    }
}
