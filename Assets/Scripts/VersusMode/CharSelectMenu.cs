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
        }

        // Update is called once per frame
        void Update()
        {
            // check if players are locked in
            if (p1Selector.lockedIn && p2Selector.lockedIn)
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

                Storage.battler1 = p1Selector.selectedBattler;
                Storage.battler2 = p2Selector.selectedBattler;

                // TODO: make player/cpu state depend on gamemode
                Storage.isPlayer1 = true;
                Storage.isPlayer2 = true;

                Storage.level = null;
                Storage.gamemode = Storage.GameMode.Versus;

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