using UnityEngine;
using UnityEngine.InputSystem;

using Battle;
using Battle.Board;

namespace Multiplayer {
    public class PlayerConnectionManager : MonoBehaviour {
        public static PlayerConnectionManager instance;

        [SerializeField] private GameBoard[] boards;

        [SerializeField] private bool isBattle;


        private void Awake() {
            if (instance == null) {
                instance = this;
            } else {
                // If within a battle scene, this means that this is loading in while switching between charselect > battle.
                // Connect each controller to its respective board.
                if (isBattle) {
                    foreach (var playerInput in instance.transform.GetComponentsInChildren<PlayerInput>()) {
                        ConnectControllerToBoard(playerInput);
                    }
                }
                
                // after all this, destroy this gameobject since the existing one should be the only one.
                Destroy(gameObject);
            }
        }
        
        public void OnPlayerJoined(PlayerInput playerInput) {
            // for persistence between scenes (and organization), parent to this object
            playerInput.transform.SetParent(transform);

            if (boards != null) {
                ConnectControllerToBoard(playerInput);
            }
        }

        // upon joining at beginning or during battle, connect to a board based on player index. 0 will be left, 1 will be right
        public void ConnectControllerToBoard(PlayerInput playerInput) {
            
            var controller = playerInput.GetComponent<Controller>();
            controller.SetBoard(boards[playerInput.playerIndex]);
        }
    }
}