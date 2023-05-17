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
        private double timer;
        private bool countdownStarted = false;
        private double maxTime = 1.0;

        // Start is called before the first frame update
        void Start()
        {
            Selectable firstSelection = grid.GetChild(0).GetComponent<Selectable>();

            p1Selector.SetSelection(firstSelection);
            p2Selector.SetSelection(firstSelection);

            p1Selector.doGhostPiece = PlayerPrefs.GetInt("drawGhostPiece", 1) == 1;
            p2Selector.doGhostPiece = PlayerPrefs.GetInt("drawGhostPieceP2", 1) == 1;
        }

        // Update is called once per frame
        void Update()
        {
            // check if players are locked in, or p1 is locked in solo
            if (p1Selector.lockedIn && (p2Selector.lockedIn || Storage.gamemode == Storage.GameMode.Solo))
            {
                // both players are ready'd
                if (!countdownStarted)
                {
                    countdownStarted = true;
                    timer = maxTime;
                }
            }
            else
            {
                // one or neither is ready'd
                if (countdownStarted){
                    countdownStarted = false;
                    timer = 0.0;
                }
            }

            // update timer, if applicable
            if (countdownStarted){
                timer -= Time.deltaTime;
                
            }

            // when time reached
            if (timer <= 0 && countdownStarted)
            {
                timer = 0;
                countdownStarted = false;

                

                // if (Storage.isPlayer1 == null) Storage.isPlayer1 = true;
                // if (Storage.isPlayer2 == null) Storage.isPlayer2 = true;

                PlayerPrefs.SetInt("drawGhostPiece", p1Selector.doGhostPiece ? 1 : 0);

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

                if (!transitionHandler) {
                    Debug.LogError("Transition handler not found in scene!");
                    return;
                }

                transitionHandler.WipeToScene("ManaCycle");
            }
        }
        
        void OnValidate() {
            transitionHandler = GameObject.FindObjectOfType<TransitionScript>();
        }
    }
}