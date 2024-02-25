using Battle.Board;
using UnityEngine;

// Class for ensuring packet safety in online multiplayer
public class BoardSyncAssertion : MonoBehaviour {
    [SerializeField] private GameBoard board;
    
    private void Start() {
        // not needed in singleplayer
        if (!Storage.online) {
            Destroy(this);
        }
    }

    /// <summary>
    /// Index of last piece dropped.
    /// </summary>
    int lastPieceDroppedIndex;

    /// <summary>
    /// Asserts that the order of pieces dropped has not been jumbled over the network.
    /// If an index lower than the last dropped index is received, the board is desynced on this client; will show an error message.
    /// </summary>
    /// <param name="pieceDroppedIndex">the drop index of the piece that was just dropped</param>
    public void AssertCorrectPieceDropOrder(int pieceDroppedIndex) {

        if (pieceDroppedIndex < lastPieceDroppedIndex) {
            Debug.LogAssertion("Piece drop order is desynced! "+lastPieceDroppedIndex+" dropped before "+pieceDroppedIndex);
        }
        if (pieceDroppedIndex != lastPieceDroppedIndex+1) {
            Debug.LogAssertion("A piece drop may have been skipped! "+lastPieceDroppedIndex+" dropped before "+pieceDroppedIndex);
        }

        lastPieceDroppedIndex = pieceDroppedIndex;
    }
}