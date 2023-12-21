using Battle;
using Battle.Board;
using UnityEngine;

// literally just a script to destroy this if not in solo mode.
// this script is on the SoloBoardController in the battle scene
// also sets set gameboard of the controller component
public class SoloBoardController : MonoBehaviour {
    [SerializeField] private GameBoard board;

    private void Start() {
        // Only use this object if there is no second player and no need for multiple device handling. (PlayerConnectionHandler will destroy itself if not)
        if (Storage.isPlayerControlled2) Destroy(gameObject);

        GetComponent<Controller>().SetBoard(board);
        if (!Storage.isPlayerControlled1 && !Storage.level) GetComponent<Controller>().canControlBoard = false;
    }
}