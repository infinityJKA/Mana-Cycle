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
        
        // Get image and set color from the list in this scene's cycle
        GetComponent<Image>().color =
        GameObject.Find("Cycle").GetComponent<ManaCycle>().GetManaColors()[((int)color)];
    }
}
