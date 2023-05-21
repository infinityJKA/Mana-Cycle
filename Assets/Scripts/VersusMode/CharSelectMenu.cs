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

        // Start is called before the first frame update
        void Start()
        {
            Selectable firstSelection = grid.GetChild(0).GetComponent<Selectable>();

            p1Selector.SetSelection(firstSelection);
            p2Selector.SetSelection(firstSelection);

            p1Selector.doGhostPiece = PlayerPrefs.GetInt("drawGhostPiece", 1) == 1;
            p2Selector.doGhostPiece = PlayerPrefs.GetInt("drawGhostPieceP2", 1) == 1;

            if (Storage.gamemode == Storage.GameMode.Solo && Storage.level.lives != -1) {
                p1Selector.SetLives(Storage.level.lives);
                p1Selector.livesSelectable.gameObject.SetActive(false);
            } else {
                p1Selector.SetLives(PlayerPrefs.GetInt("versusLives", 3));
            }

            if (!Storage.isPlayerControlled1 && !Storage.isPlayerControlled2 && Storage.level == null) {
                p1Selector.cpuLevel = PlayerPrefs.GetInt("CpuVsCpuP1Level", 5);
                p2Selector.cpuLevel = PlayerPrefs.GetInt("CpuVsCpuP2Level", 5);
            } else if (Storage.isPlayerControlled1 && !Storage.isPlayerControlled2 && Storage.level == null) {
                p2Selector.cpuLevel = PlayerPrefs.GetInt("CpuLevel", 5);
            }

            transitionHandler = GameObject.FindObjectOfType<TransitionScript>();
        }

        void Update() {
            bool ready = p1Selector.lockedIn && (p2Selector.lockedIn || Storage.gamemode == Storage.GameMode.Solo);

            // while both ready, blink the ready gameobject
            readyObject.SetActive(ready && Mathf.PingPong(Time.time, 0.4f) > 0.125f);
        }

        // Called when player casts while locked in. If both players are ready, match will begin
        public void StartIfReady() {
            bool ready = p1Selector.lockedIn && (p2Selector.lockedIn || Storage.gamemode == Storage.GameMode.Solo);
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
                PlayerPrefs.SetInt("CpuVsCpuP1Level", p1Selector.cpuLevel);
                PlayerPrefs.SetInt("CpuVsCpuP2Level", p2Selector.cpuLevel);
            } else if (!p1Selector.isCpuCursor && p2Selector.isCpuCursor) {
                PlayerPrefs.SetInt("CpuLevel", p2Selector.cpuLevel);
            }
            

            if (!transitionHandler) {
                Debug.LogError("Transition handler not found in scene!");
                return;
            }

            transitionHandler.WipeToScene("ManaCycle");
        }
    }
}