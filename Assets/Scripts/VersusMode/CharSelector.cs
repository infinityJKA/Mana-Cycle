using UnityEngine;
using UnityEngine.UI;

using Sound;

namespace VersusMode {
    /// <summary>
    ///     Controls the box to the left/right of the character icon grid in the character select menu. 
    ///     This selector also controls the cursor on the character select grid through this script.
    /// </summary>
    public class CharSelector : MonoBehaviour {
        ///<summary>True for player 1, false for player 2.</summary>
        [SerializeField] private bool isPlayer1;

        ///<summary>Input script used to move the cursor and select character</summary>
        [SerializeField] private InputScript inputScript;

        [SerializeField] private Image portrait;
        [SerializeField] private TMPro.TextMeshProUGUI nameText;

        ///<summary>SFX played when interacting with menu</summary>
        [SerializeField] private AudioClip switchSFX, noswitchSFX, selectSFX, unselectSFX;

        ///<summary> Currently selected icon's Selectable component </summary>
        private CharacterIcon selectedIcon;

        ///<summary>True when the player has locked in their choice
        public bool lockedIn {get; private set;}

        // cached on validate
        private TransitionScript transitionHandler;

        public Battle.Battler selectedBattler { get { return selectedIcon.battler; }}

        void Start() {
            RefreshLockVisuals();
            
        }

        void Update() {
            // Move cursor if not locked in
            if (!lockedIn) 
            {
                // Look for a new icon to select in inputted directions, select if found
                if (Input.GetKeyDown(inputScript.Left)) SetSelection(selectedIcon.selectable.FindSelectableOnLeft());
                else if (Input.GetKeyDown(inputScript.Right)) SetSelection(selectedIcon.selectable.FindSelectableOnRight());
                else if (Input.GetKeyDown(inputScript.Up)) SetSelection(selectedIcon.selectable.FindSelectableOnUp());
                else if (Input.GetKeyDown(inputScript.Down)) SetSelection(selectedIcon.selectable.FindSelectableOnDown());
            }

            // Lock in or un-lock in when cast is pressed
            if (Input.GetKeyDown(inputScript.Cast)) ToggleLock();

            // unlock in when pause pressed, or go back to menu if not locked in
            if (Input.GetKeyDown(inputScript.Pause)) 
            {
                if (lockedIn) ToggleLock();
                else {
                    if (!transitionHandler) {
                        Debug.LogError("Transition handler not found in scene!");
                        return;
                    }
                    transitionHandler.WipeToScene("MainMenu", reverse: true);
                }
            }
        }

        void ToggleLock()
        {
            lockedIn = !lockedIn;
            SoundManager.Instance.PlaySound(lockedIn ? selectSFX : unselectSFX);
            RefreshLockVisuals();
        }

        void RefreshLockVisuals() {
            if (lockedIn){
                portrait.color = new Color(1.0f, 1.0f, 1.0f, 1f);
                nameText.fontStyle = TMPro.FontStyles.Bold;
            }
            else {
                portrait.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                nameText.fontStyle = TMPro.FontStyles.Normal;
            }
        }

        public void SetSelection(Selectable newSelection) {
            if (!newSelection) {
                if (Application.isPlaying) SoundManager.Instance.PlaySound(noswitchSFX, 2.5f);
                return;
            }

            CharacterIcon newSelectedIcon = newSelection.GetComponent<CharacterIcon>();
            if (!newSelectedIcon) {
                Debug.LogError("CharacterIcon component not found on new cursor selectable");
                return;
            }

            if (selectedIcon) selectedIcon.SetSelected(isPlayer1, false);
            newSelectedIcon.SetSelected(isPlayer1, true);

            SoundManager.Instance.PlaySound(switchSFX, 2.5f);

            selectedIcon = newSelectedIcon;

            portrait.sprite = selectedBattler.sprite;
            nameText.text = selectedBattler.displayName;
        }

        void OnValidate() {
            transitionHandler = GameObject.FindObjectOfType<TransitionScript>();
        }
    }
}