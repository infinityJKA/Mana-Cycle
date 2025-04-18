using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Board {
    public class PiecePreview : MonoBehaviour
    {
        // List of pieces below the next piece box
        [SerializeField] private Transform previewListTransform;
        // Next piece box where the next piece is stored above this piece preview
        [SerializeField] private Transform nextPieceTransform;

        // The board this piece preview is for
        [SerializeField] private GameBoard board;
        // Piece prefab, containing an object with Tile gameobject children
        [SerializeField] private Piece piecePrefab;
        [SerializeField] private Piece singlePiecePrefab;

        // Length of the preview list
        public const int previewLength = 2;

        /// <summary>
        /// The INDEX of the current piece - counts up from 1 each time a new piece is spawned (skips 0), each piece will have a unique index.
        /// </summary>
        public int pieceIdIndex {get; private set;}

        /// <summary>
        /// Returns the next available piece ID by incrementing the index by 1 and returning that value.
        /// </summary>
        /// <returns>the next available piece ID to assign to a piece</returns>
        public int NextPieceId() {
            pieceIdIndex++;
            return pieceIdIndex;
        }

        /// <summary>
        /// Retrieve the piece that is currently in th enext piece slot on the piece preview
        /// </summary>
        public Piece GetNextPiece() {
            return nextPieceTransform.GetChild(0).GetComponent<Piece>();
        }

        /// <summary>
        /// Get a piece that is NOT in the next slot but is in the list of upcoming tiles below that, from highest/closest to lowest/furthest.
        /// </summary>
        public Piece GetPreviewPiece(int index) {
            return previewListTransform.GetChild(index).GetComponent<Piece>();
        }

        public void Setup(GameBoard board)
        {
            this.board = board;

            pieceIdIndex = -1;

            // populate the preview list & next piece box; add n pieces to list and set next piece
            CreateNewPiece(nextPieceTransform);
            for (int i=0; i<previewLength; i++) CreateNewPiece(previewListTransform);
        }

        // Spawns the next piece onto the game board and advances the list.
        // Return the piece that was added.
        public Piece SpawnNextPiece()
        {
            // Spawn the next piece on the baord and move it to the spawn location
            Piece nextPiece = GetNextPiece();
            // Debug.Log("The next piece is " + nextPiece);
            nextPiece.transform.SetParent(board.pieceBoard.transform, false);
            nextPiece.MoveTo(3,4);

            // Set the next next piece
            previewListTransform.GetChild(0).SetParent(nextPieceTransform, false);
            nextPieceTransform.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);

            // Add a new piece to preview list
            CreateNewPiece(previewListTransform);

            return nextPiece;
        }

        Piece CreateNewPiece(Transform parent)
        {
            // Add a new piece to the queue
            Piece newPiece = Instantiate(piecePrefab, Vector3.zero, Quaternion.identity, parent);
            newPiece.id = NextPieceId();
            newPiece.transform.localPosition = new Vector3(0, 0, 0);

            // Randomize the piece's tiles' colors
            newPiece.Randomize(board);
            return newPiece;
        }

        public void ReplaceNextPiece(Piece newPiece)
        {
            Destroy(nextPieceTransform.GetChild(0).gameObject);
            newPiece.transform.SetParent(nextPieceTransform);
            newPiece.transform.localPosition = nextPieceTransform.GetChild(0).transform.localPosition;
            newPiece.transform.localScale = nextPieceTransform.GetChild(0).transform.localScale;
        }

        public void ReplaceUpcomingPiece(Piece newPiece, int index)
        {
            Destroy(previewListTransform.GetChild(index).gameObject);
            newPiece.transform.SetParent(previewListTransform);
            newPiece.transform.localPosition = previewListTransform.GetChild(index).transform.localPosition;
            newPiece.transform.localScale = previewListTransform.GetChild(index).transform.localScale;
            newPiece.transform.SetSiblingIndex(previewLength-index-1);
        }
    }
}