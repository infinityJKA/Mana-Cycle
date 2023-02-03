using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiecePreview : MonoBehaviour
{
    // Next piece box where the next piece is stored abo e this piece preview
    [SerializeField] private Transform nextPieceBox;
    // The board this piece preview is for
    [SerializeField] private GameBoard board;
    // Piece prefab, containing an object with Tile gameobject children
    [SerializeField] private Piece piecePrefab;

    public void Setup(GameBoard board)
    {
        this.board = board;

        // Add five pieces to list and one to next piece
        NewPiece(nextPieceBox);
        for (int i=0; i<5; i++) NewPiece(transform);
    }

    // Spawns the next piece onto the game board and advances the list.
    // Return the piece that was added.
    public Piece SpawnNextPiece()
    {
        // Spawn the next piece on the baord and move it to the spawn location
        Piece nextPiece = nextPieceBox.GetChild(0).GetComponent<Piece>();
        nextPiece.transform.SetParent(board.transform, false);
        nextPiece.MoveTo(3,1);

        // Set the next next piece
        transform.GetChild(0).SetParent(nextPieceBox, false);

        // Add new piece to preview list
        NewPiece(transform);

        return nextPiece;
    }

    Piece NewPiece(Transform parent)
    {
        // Add a new piece to the queue
        // The new tiles' position Vector2 will determine how it is rendered.
        Piece newPiece = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity, parent);

        // Randomize the piece's tiles' colors
        newPiece.Randomize(board);
        return newPiece;
    }
}
