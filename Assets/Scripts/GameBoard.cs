using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public static readonly int height = 8;
    public static readonly int width = 14;
    public static readonly int tilePixelSize = 25;

    // Board containing all tiles that have been placed and their colors. NONE is an empty space (from ManaColor enum).
    private ManaColor[,] board;
    // Piece that is currently being dropped.
    private Piece currentPiece;

    // Start is called before the first frame update
    void Start()
    {
        board = new ManaColor[width, height];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Create a new piece and spawn it at the top of the board
    public void NewPiece()
    {
        // Creates a new piece at the spawn location with randomized colors (check Piece constructor)
        currentPiece = new Piece(); 
    }
}
