using UnityEngine;
using UnityEngine.InputSystem;

using Battle;
using Battle.Board;
using VersusMode;

namespace Multiplayer {
    public class PlayerConnectionManager : MonoBehaviour {
        public static PlayerConnectionManager instance;

        [SerializeField] private GameBoard[] boards;

        [SerializeField] private CharSelector[] charSelectors;

        [SerializeField] private ConnectMode connectMode;

        public enum ConnectMode {
            CharSelect,
            Battle,
            // If this mode is found, then remove all the players when this object is loaded, destroying both this and current instance
            // depending on how things go, this may be removed and main menu will have multiplayer support, or at least repsond to multiple controllers differently
            DestroyMultiplayer, 
        }

        // if player joined script should reparent to currently invoked connection manager or not
        // (OnPlayerJoined can only have 1 argument because it's called by a unity event in Player Input Manager)
        private bool reparent = false;

        private void Awake() {
            // destroy self if not a multiplayer mode.
            if (!Storage.isPlayerControlled2) Destroy(gameObject);

            if (instance == null) {
                if (connectMode == ConnectMode.DestroyMultiplayer) {
                    Destroy(gameObject);
                } else {
                    instance = this;
                    reparent = true;
                    DontDestroyOnLoad(gameObject);
                }
            } else {
                reparent = false;

                if (connectMode == ConnectMode.DestroyMultiplayer) {
                    Destroy(instance.gameObject);
                } else {
                    foreach (var playerInput in instance.transform.GetComponentsInChildren<PlayerInput>()) {
                        OnPlayerJoined(playerInput);
                    }
                }
                
                // after all this tomfoolery, destroy this gameobject since the existing one should be the only one.
                Destroy(gameObject);
            }
        }
        
        // Initialize based on THIS connection manager's connect mode.
        // Will not reparent when called from awake while an instance already exists in which all the player inputs are parented
        // however this will contain info needed by the existing connection manager to set up connections before the new one is destroyed
        public void OnPlayerJoined(PlayerInput playerInput) {
            // for persistence between scenes (and organization), parent to this object
            if (reparent) playerInput.transform.SetParent(transform);

            if (connectMode == ConnectMode.CharSelect) {
                ConnectControllerToCharSelector(playerInput);
            }

            else if (connectMode == ConnectMode.Battle) {
                ConnectControllerToBoard(playerInput);
            }
        }

        // upon joining at beginning or during battle, connect to a board based on player index. 0 will be left, 1 will be right
        public void ConnectControllerToBoard(PlayerInput playerInput) {
            
            var controller = playerInput.GetComponent<Controller>();
            controller.SetBoard(boards[playerInput.playerIndex]);
        }

        public void ConnectControllerToCharSelector(PlayerInput playerInput) {
            var controller = playerInput.GetComponent<Controller>();
            controller.SetCharSelector(charSelectors[playerInput.playerIndex]);
        }


        public void DisableControllers() {
            foreach (var playerInput in instance.transform.GetComponentsInChildren<PlayerInput>()) {
                playerInput.DeactivateInput();
            }
        }

        public void EnableControllers() {
            foreach (var playerInput in instance.transform.GetComponentsInChildren<PlayerInput>()) {
                playerInput.ActivateInput();
            }
        }
    }
}