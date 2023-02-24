using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePreview : MonoBehaviour
{
    // Next piece box where the next piece is stored above this piece preview
    [SerializeField] private Transform nextPieceBox;
    // The board this piece preview is for
    [SerializeField] private GameBoard board;
    // Piece prefab, containing an object with Tile gameobject children
    [SerializeField] private Piece piecePrefab;
    // Length of the preview list
    [SerializeField] private int previewLength = 4;

    public void Setup(GameBoard board)
    {
        this.board = board;

        // populate the preview list & next piece box; add five pieces to list and one to next piece
        CreateNewPiece(nextPieceBox);
        for (int i=0; i<previewLength; i++) CreateNewPiece(transform);
    }

    // Spawns the next piece onto the game board and advances the list.
    // Return the piece that was added.
    public Piece SpawnNextPiece()
    {
        // Spawn the next piece on the baord and move it to the spawn location
        Piece nextPiece = nextPieceBox.GetChild(0).GetComponent<Piece>();
        // Debug.Log("The next piece is " + nextPiece);
        nextPiece.transform.SetParent(board.pieceBoard.transform, false);
        nextPiece.MoveTo(3,1);

        // Set the next next piece
        transform.GetChild(0).SetParent(nextPieceBox, false);

        // Add a new piece to preview list
        CreateNewPiece(transform);

        return nextPiece;
    }

    Piece CreateNewPiece(Transform parent)
    {
        // Add a new piece to the queue
        Piece newPiece = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity, parent);

        // Randomize the piece's tiles' colors
        newPiece.Randomize(board);
        return newPiece;
    }

}
