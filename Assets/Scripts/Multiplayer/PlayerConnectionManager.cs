using UnityEngine;
using UnityEngine.InputSystem;

using Battle;
using Battle.Board;
using VersusMode;
using Mirror;

namespace Multiplayer {
    public class PlayerConnectionManager : MonoBehaviour {
        public static PlayerConnectionManager instance;

        [SerializeField] private GameBoard[] boards = new GameBoard[2];

        [SerializeField] private CharSelector[] charSelectors = new CharSelector[2];

        [SerializeField] public ConnectMode connectMode;

        [SerializeField] private GameObject controllerPrefab;

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

            // destroy self if not a multiplayer mode or in online mode where only player 1 will control
            if (Storage.gamemode == Storage.GameMode.Solo || !Storage.isPlayerControlled2 || Storage.online) Destroy(gameObject);
    
            if (instance == null) {
                if (connectMode == ConnectMode.DestroyMultiplayer) {
                    Destroy(gameObject);
                    if (NetworkClient.activeHost) NetworkManager.singleton.StopHost();
                    else if (NetworkClient.active) NetworkManager.singleton.StopClient();
                    if (NetworkManager.singleton) Destroy(NetworkManager.singleton.gameObject);
                } else {
                    instance = this;
                    reparent = true;
                    DontDestroyOnLoad(gameObject);

                    // if char seelct and player vs player, disconnect all at start, they will be connected to by controllers
                    if (connectMode == ConnectMode.CharSelect && Storage.isPlayerControlled1 && Storage.isPlayerControlled2) {
                        foreach (var charSelector in charSelectors) {
                            charSelector.Disconnect();
                        }
                    }
                }
            } else {
                reparent = false;
                instance.connectMode = connectMode;

                if (connectMode == ConnectMode.DestroyMultiplayer) {
                    Debug.Log("Destroying...");
                    Destroy(instance.gameObject);
                } else {
                    // if in dual keyboard mode, give each board a controller
                    if (Storage.useDualKeyboardInput) {
                        foreach (var board in boards) {
                            
                            GameObject controllerObject = Instantiate(controllerPrefab);
                            Destroy(controllerObject.GetComponent<PlayerInput>());

                            Controller controller = controllerObject.GetComponent<Controller>();
                            controller.SetBoard(board);
                            controller.EnableInputScripts();
                        }
                    }
                    else {
                        foreach (var playerInput in instance.transform.GetComponentsInChildren<PlayerInput>()) {
                            OnPlayerJoined(playerInput);
                        } 
                    }
                }
                
                // after all this tomfoolery, destroy this gameobject since the existing one should be the only one.
                Destroy(gameObject);
            }

            if (connectMode == ConnectMode.Battle)
            {
                instance.boards[0] = GameObject.Find("Board").GetComponent<GameBoard>();
                instance.boards[1] = GameObject.Find("Enemy Board").GetComponent<GameBoard>();
            }

            if (connectMode == ConnectMode.CharSelect)
            {
                Debug.Log("bruh " + GameObject.Find("P1Selector").GetComponent<CharSelector>());
                instance.charSelectors[0] = GameObject.Find("P1Selector").GetComponent<CharSelector>();
                instance.charSelectors[1] = GameObject.Find("P2Selector").GetComponent<CharSelector>();
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

        public void OnDisconnect(PlayerInput playerInput) {
            if (!enabled) return;
            DisconnectCharSelector(playerInput);
        }

        // upon joining at beginning or during battle, connect to a board based on player index. 0 will be left, 1 will be right
        public void ConnectControllerToBoard(PlayerInput playerInput) {
            if (playerInput.playerIndex >= boards.Length) return;

            var controller = playerInput.GetComponent<Controller>();

            // if in dual keyboard mode, enable the input scripts on the controller
            if (Storage.useDualKeyboardInput) {
                controller.EnableInputScripts();
            }

            controller.SetBoard(boards[playerInput.playerIndex]);
        }

        public void ConnectControllerToCharSelector(PlayerInput playerInput) {
            if (playerInput.playerIndex >= charSelectors.Length) return;
            var controller = playerInput.GetComponent<Controller>();
            var charSelector = charSelectors[playerInput.playerIndex];
            controller.SetCharSelector(charSelector);
            charSelector.Connect();
        }

        public void DisconnectCharSelector(PlayerInput playerInput) {
            if (playerInput.playerIndex >= charSelectors.Length) return;
            if (connectMode == ConnectMode.CharSelect) {
                var charSelector = charSelectors[playerInput.playerIndex];
                charSelector.Disconnect();
            }
        }

        public void DisableControllers() {
            foreach (var playerInput in instance.transform.GetComponentsInChildren<PlayerInput>()) {
                playerInput.DeactivateInput();
                Destroy(playerInput.gameObject);
            }
            GetComponent<PlayerInputManager>().DisableJoining();
            foreach (var selector in charSelectors) {
                selector.Connect();
            }
        }

        public void EnableControllers() {
            foreach (var selector in charSelectors) {
                selector.Disconnect();
            }
            GetComponent<PlayerInputManager>().EnableJoining();
            // foreach (var playerInput in instance.transform.GetComponentsInChildren<PlayerInput>()) {
            //     playerInput.ActivateInput();
            // }
        }
    }
}