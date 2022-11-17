using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    // Color of this title. (Public read but no public write)
    private ManaColor color;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ManaColor GetManaColor()
    {
        return color;
    }

    public void Randomize()
    {
        // Randomly choose color from color enum length
        color = (ManaColor) Random.Range(0,5);
        // go up three parents (rotationCenter -> piece -> gameboard),
        GetComponent<Image>()
        .color = transform.parent.transform.parent.transform.parent
        // and access the mana colors list on the gameboard to get the Color to tint this object
            .GetComponent<GameBoard>().GetManaColors()[((int)color)];
    }
}
