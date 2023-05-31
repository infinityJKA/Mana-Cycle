using UnityEngine;
using UnityEngine.UI;
namespace VersusMode {
    ///<summary> Controls the character selection menu and the cursors within it. </summary>
    public class CharSelectMenu : MonoBehaviour {
        ///<summary>Selectors for both players</summary>
        [SerializeField] private CharSelector p1Selector, p2Selector;
        ///<summary>The grid of characters to select with the selectorss defined in this object</summary>
        [SerializeField] private Transform grid;

        private TransitionScript transitionHandler;

        // Gameobject to show when both players are ready and game is ready to start
        [SerializeField] private GameObject readyObject;

        // Sound to play when both players are ready and match is starting
        [SerializeField] private AudioClip startSFX;
        
        // Start button - only used in mobile mode
        [SerializeField] private Button startButton;

        // Start text to darken when button disabled
        [SerializeField] private Selectable startText;

        [SerializeField] private bool mobile;
        public bool Mobile { get {return mobile;} }

        // (for debug purposes)
        void Awake() {
            if (Storage.gamemode == Storage.GameMode.Default) {
                Storage.gamemode = Storage.GameMode.Versus;
                Storage.isPlayerControlled1 = false;
                Storage.isPlayerControlled2 = false;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Selectable firstSelection = grid.GetChild(0).GetComponent<Selectable>();

            p2Selector.SetSelection(firstSelection);
            p1Selector.SetSelection(firstSelection);

            p2Selector.MenuInit();
            p1Selector.MenuInit();

            p1Selector.doGhostPiece = PlayerPrefs.GetInt("drawGhostPiece", 1) == 1;
            p2Selector.doGhostPiece = PlayerPrefs.GetInt("drawGhostPieceP2", 1) == 1;

            if (Storage.gamemode == Storage.GameMode.Solo && Storage.level.lives != -1) {
                p1Selector.SetLives(Storage.level.lives);
                p1Selector.livesSelectable.gameObject.SetActive(false);
            } else {
                p1Selector.SetLives(PlayerPrefs.GetInt("versusLives", 3));
            }

            if (!Storage.isPlayerControlled1 && !Storage.isPlayerControlled2 && Storage.level == null) {
                p1Selector.CpuLevel = PlayerPrefs.GetInt("CpuVsCpuP1Level", 5);
                p2Selector.CpuLevel = PlayerPrefs.GetInt("CpuVsCpuP2Level", 5);
            } else if (Storage.isPlayerControlled1 && !Storage.isPlayerControlled2 && Storage.level == null) {
                p2Selector.CpuLevel = PlayerPrefs.GetInt("CpuLevel", 5);
            }

            transitionHandler = GameObject.FindObjectOfType<TransitionScript>();

            RefreshStartButton();
        }

        bool ready { 
            get { 
                return p1Selector.lockedIn && (p2Selector.lockedIn || Storage.gamemode == Storage.GameMode.Solo);
            } 
        }

        void Update() {
            // while both ready, blink the ready gameobject
            if (!mobile) readyObject.SetActive(ready && Mathf.PingPong(Time.time, 0.4f) > 0.125f);
        }

        // Called when player casts while locked in. If both players are ready, match will begin
        public void StartIfReady() {
            if (ready) {
                Sound.SoundManager.Instance.PlaySound(startSFX, 0.5f);
                StartMatch();
            }
        }

        void StartMatch() {
            // if (Storage.isPlayer1 == null) Storage.isPlayer1 = true;
            // if (Storage.isPlayer2 == null) Storage.isPlayer2 = true;

            PlayerPrefs.SetInt("drawGhostPiece", p1Selector.doGhostPiece ? 1 : 0);
            PlayerPrefs.SetInt("enableAbilities", p1Selector.enableAbilities ? 1 : 0);

            if (!Storage.level) PlayerPrefs.SetInt("versusLives", p1Selector.lives);
            Storage.lives = p1Selector.lives;

            if (Storage.gamemode != Storage.GameMode.Solo)
            {
                Storage.battler1 = p1Selector.selectedBattler;
                Storage.battler2 = p2Selector.selectedBattler;
                Storage.level = null;
                Storage.gamemode = Storage.GameMode.Versus;
                PlayerPrefs.SetInt("drawGhostPieceP2", p2Selector.doGhostPiece ? 1 : 0);
            }
            else 
            {
                Storage.level.battler = p1Selector.selectedBattler;
            }

            if (p1Selector.isCpuCursor && p2Selector.isCpuCursor) {
                PlayerPrefs.SetInt("CpuVsCpuP1Level", p1Selector.CpuLevel);
                PlayerPrefs.SetInt("CpuVsCpuP2Level", p2Selector.CpuLevel);
            } else if (!p1Selector.isCpuCursor && p2Selector.isCpuCursor) {
                PlayerPrefs.SetInt("CpuLevel", p2Selector.CpuLevel);
            }
            

            if (!transitionHandler) {
                Debug.LogError("Transition handler not found in scene!");
                return;
            }

            transitionHandler.WipeToScene("ManaCycle");
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
    }
}