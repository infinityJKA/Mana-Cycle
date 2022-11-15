using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public static readonly int height = 8;
    public static readonly int width = 14;
    public static readonly int tilePixelSize = 25;

    

    private ManaColor[,] board;

    // Start is called before the first frame update
    void Start()
    {
        board = new ManaColor[width][height];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Create a new piece and spawn it at the top of the board
    public void NewPiece()
    {

    }
}
