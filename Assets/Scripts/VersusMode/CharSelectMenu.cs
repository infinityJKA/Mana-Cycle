using System.Collections.Generic;
using Multiplayer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using Battle.Cycle;

namespace VersusMode {
    ///<summary> Controls the character selection menu and the cursors within it. </summary>
    public class CharSelectMenu : MonoBehaviour {
        public static CharSelectMenu Instance {get; private set;}

        ///<summary>Selectors for both players</summary>
        [SerializeField] public CharSelector p1Selector, p2Selector;
        ///<summary>The grid of characters to select with the selectorss defined in this object</summary>
        [SerializeField] private Transform grid;
        public CharacterIcon[] characterIcons {get; private set;}

        private TransitionScript transitionHandler;

        // Gameobject to show when both players are ready and game is ready to start
        [SerializeField] private GameObject readyBlinkObject;
        [SerializeField] private GameObject readyHoldObject;

        // Sound to play when both players are ready and match is starting
        [SerializeField] private GameObject startSFX;
        
        // Start button - only used in mobile mode
        [SerializeField] private Button startButton;

        // Start text to darken when button disabled
        [SerializeField] private Selectable startText;

        [SerializeField] private CharSelectCursor p1Cursor, p2Cursor, cpuCursor;

        // for handling Controllers
        // [SerializeField] private PlayerConnectionManager connectionManager;

        [SerializeField] private Button dualKeyboardButton;

        [SerializeField] private bool mobile;
        public bool Mobile { get {return mobile;} }

        public bool started {get; private set;}
        // (for debug purposes)
        void Awake() {
            Instance = this;

            // if (!connectionManager) {
            //     // connectionManager = FindFirstObjectByType<PlayerConnectionManager>();
            //     connectionManager = PlayerConnectionManager.instance;
            // }

            characterIcons = GetComponentsInChildren<CharacterIcon>();
            for (int i = 0; i < characterIcons.Length; i++) {
                characterIcons[i].SetIndex(i);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Selectable firstSelection = grid.GetChild(0).GetComponent<Selectable>();

            // p2Selector.SetSelection(firstSelection);
            // if (!p2Selector.connected) p2Selector.HideSelection();
            // p1Selector.SetSelection(firstSelection);
            // if (!p1Selector.connected) p1Selector.HideSelection();

            p2Selector.MenuInit();
            p1Selector.MenuInit();

            p1Selector.doGhostPiece = Settings.current.drawGhostPiece;
            p2Selector.doGhostPiece = Settings.current.drawGhostPieceP2;

            if (Storage.gamemode == Storage.GameMode.Solo && Storage.level.lives != -1) {
                p1Selector.SetLives(Storage.level.lives);
                p1Selector.livesSelectable.gameObject.SetActive(false);
            } else {
                p1Selector.SetLives(Settings.current.versusLives);
            }


            cpuCursor.gameObject.SetActive(Storage.level == null && !Storage.isPlayerControlled2);
            p2Cursor.gameObject.SetActive(Storage.level == null && Storage.isPlayerControlled2);
            // cpu vs cpu
            if (!Storage.isPlayerControlled1 && !Storage.isPlayerControlled2 && Storage.level == null) 
            {
                p1Selector.CpuLevel = Settings.current.cvcP1Level;
                p2Selector.CpuLevel = Settings.current.cvcP2Level;

                p2Cursor.gameObject.SetActive(true);
                cpuCursor.gameObject.SetActive(false);
            } 
            // player vs cpu
            else if (Storage.isPlayerControlled1 && !Storage.isPlayerControlled2 && Storage.level == null) 
            {
                p2Selector.CpuLevel = Settings.current.cpuLevel;
            }

            RefreshStartButton();

            if (Storage.useDualKeyboardInput) {
                SwitchToDualKeyboardMode();
            }

            if (Storage.online) {
                OnlineMenu.singleton.ShowOnlineMenu();
            }
        }

        bool ready { 
            get { 
                return p1Selector.lockedIn && (p2Selector.lockedIn || Storage.gamemode == Storage.GameMode.Solo);
            } 
        }

        void Update() {
            // while both ready, blink the ready gameobject
            if (!mobile) readyBlinkObject.SetActive(ready && Mathf.PingPong(Time.time, 0.4f) > 0.125f);
            if (!mobile) readyHoldObject.SetActive(ready);
        }

        // Called when player casts while locked in. If both players are ready, match will begin
        public void StartIfReady() {

            // Only the host can start the match
            if (ready) {
                started = true;
                Instantiate(startSFX);
                StartMatch();
            }
        }

        public bool IsBothPlayersReady() {
            return ready;
        }

        void StartMatch() {
            // if (Storage.isPlayer1 == null) Storage.isPlayer1 = true;
            // if (Storage.isPlayer2 == null) Storage.isPlayer2 = true;

            // Store selected settings
            Settings.current.drawGhostPiece = p1Selector.doGhostPiece;

            if (Storage.gamemode == Storage.GameMode.Versus && !Storage.level) {
                Settings.current.enableAbilities = p1Selector.enableAbilities;
                Settings.current.versusLives = p1Selector.lives;

                Settings.current.drawGhostPieceP2 = p2Selector.doGhostPiece;

                if (p1Selector.isCpuCursor && p2Selector.isCpuCursor) {
                    Settings.current.cvcP1Level = p1Selector.CpuLevel;
                    Settings.current.cvcP2Level = p2Selector.CpuLevel;
                } else if (!p1Selector.isCpuCursor && p2Selector.isCpuCursor) {
                    Settings.current.cpuLevel = p2Selector.CpuLevel;
                }
            }

            Settings.Save();
            
            // Setup global storage variables
            Storage.lives = p1Selector.lives;
            if (Storage.gamemode != Storage.GameMode.Solo)
            {
                Storage.battler1 = p1Selector.selectedBattler;
                Storage.battler2 = p2Selector.selectedBattler;
                Storage.level = null;
                Storage.gamemode = Storage.GameMode.Versus;
            }
            else 
            {
                Storage.level.battler = p1Selector.selectedBattler;
            }
            
            bool dualKeyboardAvailable = Storage.gamemode == Storage.GameMode.Versus && !Storage.online;
            dualKeyboardButton.gameObject.SetActive(dualKeyboardAvailable);

            Instantiate(startSFX);
            started = true;
            // don't auto fade into battle if online; need to wait for battle initialization to be sent/received
            bool autoFadeOut = !Storage.online;

            ManaCycle.initializeFinished = false;
            TransitionScript.instance.WipeToScene("ManaCycle", autoFadeOut: autoFadeOut);
        }

        // ---- Mobile ----
        public void RefreshStartButton() {
            if (!mobile) return;
            // startButton.interactable = ready;
            // startText.interactable = ready;

            if (Storage.gamemode == Storage.GameMode.Versus && p1Selector.Active) {
                startText.GetComponent<TMPro.TextMeshProUGUI>().text = "SELECT";
            } else {
                startText.GetComponent<TMPro.TextMeshProUGUI>().text = "START!";
            }
        }

        // Called when character icons are pressed from the grid
        public void SetSelection(Selectable selectable) {
            if (p1Selector.Active) {
                p1Selector.SetSelection(selectable);
            } else {
                p2Selector.SetSelection(selectable);
            }
        }

        // Called when the start/select button is pressed in mobile. 
        // Locks in current character if p1 and p2 also has to select;
        // starts game if p2 or p1 and p2 deosn't have to select
        public void StartButtonPressed() {
            if (p1Selector.Active) {
                if (Storage.gamemode == Storage.GameMode.Versus) {
                    p1Selector.ToggleLock();
                } else {
                    // solo mode - lock in and start
                    p1Selector.ToggleLock();
                    StartIfReady();
                }
            } else if (p2Selector.Active) {
                // p2 is ready in versus mode, lock in and start
                if (!p2Selector.lockedIn) p2Selector.ToggleLock();
                StartIfReady();
            }
        }

        public void BackButtonPressed() {
            if (p1Selector.Active) {
                p1Selector.Back();
            } else {
                p2Selector.Back();
            }
        }

        // used by on-screen buttons
        public void AdjustCpuLevel(int delta) {
            if (p1Selector.Active) {
                p1Selector.AdjustCPULevel(delta);
            } else {
                p2Selector.AdjustCPULevel(delta);
            }
        }

        bool dualKeybaord = Storage.useDualKeyboardInput;
        public void ToggleDualKeyboard() {
            dualKeybaord = !dualKeybaord;
            if (dualKeybaord) {
                SwitchToDualKeyboardMode(); 
            } else {
                SwitchToControllers();
            }
        }

        public void SwitchToDualKeyboardMode() {
            p1Selector.DualKeyboardEnabled();
            p2Selector.DualKeyboardEnabled();
            PlayerConnectionManager.instance.DisableControllers();
            Storage.useDualKeyboardInput = true;
            Debug.Log("Switched to dual keyboard mode");
        }

        public void SwitchToControllers() {
            p1Selector.DualKeyboardDisabled();
            p2Selector.DualKeyboardDisabled();
            PlayerConnectionManager.instance.EnableControllers();
            Storage.useDualKeyboardInput = false;
            Debug.Log("Switched to multi-device controller mode");
        }

        // for solo modes, get the selector that the SoloCharSelectController should currently be controlling.
        public CharSelector GetActiveSelector() {
            if (p2Selector.Active && p1Selector.lockedIn) {
                return p2Selector;
            } else {
                return p1Selector;
            }
        }
    }
}