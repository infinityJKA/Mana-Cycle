using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Sound;

namespace VersusMode {
    /// <summary>
    ///     Controls the box to the left/right of the character icon grid in the character select menu. 
    ///     This selector also controls the cursor on the character select grid through this script.
    /// </summary>
    public class CharSelector : MonoBehaviour {
        ///<summary>True for player 1, false for player 2.</summary>
        [SerializeField] private bool isPlayer1;

        ///<summary>Other player's charselector. Only used in mirroring battle preferences</summary>
        [SerializeField] private CharSelector opponentSelector;

        ///<summary>Input script used to move the cursor and select character</summary>
        [SerializeField] private InputScript inputScript;
        // set as the inputScript when in solo mode
        [SerializeField] private InputScript soloInputScript;

        [SerializeField] private Image portrait;
        [SerializeField] private TMPro.TextMeshProUGUI nameText;

        ///<summary>SFX played when interacting with menu</summary>
        [SerializeField] private AudioClip switchSFX, noswitchSFX, selectSFX, unselectSFX, infoOpenSFX, infoCloseSFX;

        /// Fade in/out speed for the ability info & settings box
        [SerializeField] private float fadeSpeed;
        /// Displacement of the ability info & settings box when fading in/out
        [SerializeField] private Vector2 fadeDisplacement;

        ///<summary>Canvas group for the ability info box</summary>
        [SerializeField] private CanvasGroup abilityInfoCanvasGroup;
        ///<summary>Text field within the ability description object that displays passive&active ability</summary>
        [SerializeField] private TMPro.TextMeshProUGUI abilityText;

        ///<summary>Canvas group for the char select settings box</summary>
        [SerializeField] private CanvasGroup settingsCanvasGroup;
        ///<summary>Toggle that toggles the ghost piece specific to this battle. Copies settings value at start</summary>
        [SerializeField] private Toggle ghostPieceToggle;

        /// tip text in the corner, p2 tip text gets hidden in solo
        [SerializeField] private GameObject tipText;

        /// character grid gameobject used to hide unavailable battlers
        [SerializeField] private GameObject battlerGrid;

        ///<summary>If the ability info screen is currently being displayed</summary>
        private bool abilityInfoDisplayed = false;
        ///<summary>If the ability info screen is currently being displayed</summary>
        private bool settingsDisplayed = false;
        ///<summary>If the ability info box is currently animating in OR out</summary>
        private bool abilityInfoAnimating;
        ///<summary>If the ability info box is currently animating in OR out</summary>
        private bool settingsAnimating;
        ///<summary>Current percentage that the ability menu is faded in/out
        private float abilityInfoFadeAmount = 0;
        ///<summary>Current percentage that the ability menu is faded in/out
        private float settingsFadeAmount = 0;

        ///<summary> Currently selected icon's Selectable component </summary>
        private CharacterIcon selectedIcon;
        ///<summary> Currently selected selectable in the settings menu </summary>
        private Selectable settingsSelection; 
        
        ///<summary>True when the player has locked in their choice
        public bool lockedIn {get; private set;}


        // cached on validate
        private TransitionScript transitionHandler;


        // properties
        public Battle.Battler selectedBattler { get { return selectedIcon.battler; }}


        private Vector2 centerPosition;
        void Start() {
            // TEMP FOR TESTING !! ,`:)
            // Storage.gamemode = Storage.GameMode.Solo;
            abilityInfoCanvasGroup.alpha = 0;
            centerPosition = abilityInfoCanvasGroup.transform.localPosition;
            RefreshLockVisuals();

            
            if (Storage.gamemode == Storage.GameMode.Solo)
            {
                // set solo mode inputs 
                // TODO change tip text depending on inputs
                inputScript = soloInputScript;

                // hide p2 elements in in solo mode
                if (!isPlayer1)
                {
                    tipText.SetActive(false);
                    gameObject.SetActive(false);
                    return;
                }

                // loop through battlers and hide battler portraits based on level available battlers
                for (int i = 0; i < battlerGrid.transform.childCount; i++)
                {
                    GameObject portrait = battlerGrid.transform.GetChild(i).gameObject;
                    // Debug.Log(portrait.name);
                    if (!Storage.level.availableBattlers.Contains(portrait.GetComponent<CharacterIcon>().battler)){
                        portrait.SetActive(false);
                    }
                }
            }

            SetSettingsSelection(ghostPieceToggle);
            ghostPieceToggle.isOn = PlayerPrefs.GetInt("drawGhostPiece", 1) == 1;
        }

        void Update() {
            if (abilityInfoAnimating) {
                // fade the ability window in/out according to state
                float abilityTarget = abilityInfoDisplayed ? 1 : 0;
                abilityInfoFadeAmount = Mathf.MoveTowards(abilityInfoFadeAmount, abilityTarget, fadeSpeed*Time.deltaTime);
                if (abilityInfoFadeAmount == abilityTarget) abilityInfoAnimating = false;
                abilityInfoCanvasGroup.alpha = abilityInfoFadeAmount;
                if (abilityInfoAnimating) {
                    abilityInfoCanvasGroup.transform.localPosition = centerPosition + (1-abilityInfoFadeAmount) * fadeDisplacement * (isPlayer1?1:-1);
                } else {
                    abilityInfoCanvasGroup.transform.localPosition = centerPosition;
                }
            }

            if (settingsAnimating) {
                float settingsTarget = settingsDisplayed ? 1 : 0;
                settingsFadeAmount = Mathf.MoveTowards(settingsFadeAmount, settingsTarget, fadeSpeed*Time.deltaTime);
                if (settingsFadeAmount == settingsTarget) settingsAnimating = false;
                settingsCanvasGroup.alpha = settingsFadeAmount;
                if (settingsAnimating) {
                    settingsCanvasGroup.transform.localPosition = centerPosition + (1-settingsFadeAmount) * fadeDisplacement * (isPlayer1?1:-1);
                } else {
                    settingsCanvasGroup.transform.localPosition = centerPosition;
                }
            }

            if (settingsDisplayed) {
                // Look for a new icon to select in inputted directions, select if found
                if (Input.GetKeyDown(inputScript.Left)) SetSettingsSelection(settingsSelection.FindSelectableOnLeft());
                else if (Input.GetKeyDown(inputScript.Right)) SetSettingsSelection(settingsSelection.FindSelectableOnRight());
                else if (Input.GetKeyDown(inputScript.Up)) SetSettingsSelection(settingsSelection.FindSelectableOnUp());
                else if (Input.GetKeyDown(inputScript.Down)) SetSettingsSelection(settingsSelection.FindSelectableOnDown());
            }

            // Move cursor if not locked in and not controlling settings menu
            else if (!lockedIn) 
            {
                // Look for a new icon to select in inputted directions, select if found
                if (Input.GetKeyDown(inputScript.Left)) SetSelection(selectedIcon.selectable.FindSelectableOnLeft());
                else if (Input.GetKeyDown(inputScript.Right)) SetSelection(selectedIcon.selectable.FindSelectableOnRight());
                else if (Input.GetKeyDown(inputScript.Up)) SetSelection(selectedIcon.selectable.FindSelectableOnUp());
                else if (Input.GetKeyDown(inputScript.Down)) SetSelection(selectedIcon.selectable.FindSelectableOnDown());
            }

            // Lock in or un-lock in when cast is pressed
            if (Input.GetKeyDown(inputScript.Cast) && !settingsDisplayed) ToggleLock();

            if (Input.GetKeyDown(inputScript.Pause)) 
            {
                // close ability info/settings menu if it is open
                if (abilityInfoDisplayed) ToggleAbilityInfo();
                else if (settingsDisplayed) ToggleSettings();
                
                // unlock in when pause pressed
                else if (lockedIn) ToggleLock();

                // or go back to menu if not locked in
                else {
                    if (!transitionHandler) {
                        Debug.LogError("Transition handler not found in scene!");
                        return;
                    }
                    transitionHandler.WipeToScene((Storage.gamemode != Storage.GameMode.Solo) ? "MainMenu" : "SoloMenu", reverse: true);
                }
            }

            // show/hide ability info when rotate CCW is pressed
            if (Input.GetKeyDown(inputScript.RotateCCW))
            {
                ToggleAbilityInfo();
            }
            if (Input.GetKeyDown(inputScript.RotateCW))
            {
                ToggleSettings();
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

        void ToggleAbilityInfo() {
            if (settingsDisplayed) ToggleSettings();
            abilityInfoDisplayed = !abilityInfoDisplayed;
            abilityInfoAnimating = true;
            SoundManager.Instance.PlaySound(abilityInfoDisplayed ? infoOpenSFX : infoCloseSFX);
        }

        void ToggleSettings() {
            if (abilityInfoDisplayed) ToggleAbilityInfo();
            settingsDisplayed = !settingsDisplayed;
            settingsAnimating = true;
            // selectedIcon.cursorImage.color = new Color(1f, 1f, 1f, settingsDisplayed ? 0.5f : 1f);
            SoundManager.Instance.PlaySound(settingsDisplayed ? infoOpenSFX : infoCloseSFX);
        }

        void SetSettingsSelection(Selectable selectable) {
            if (selectable == null || selectable == settingsSelection) return;
            if (settingsSelection) settingsSelection.OnDeselect(null);
            settingsSelection = selectable;
            settingsSelection.OnSelect(null);
        }

        public void SetSelection(Selectable newSelection) {
            if (!newSelection) {
                if (Application.isPlaying) SoundManager.Instance.PlaySound(noswitchSFX, 2.5f);
                return;
            }

            CharacterIcon newSelectedIcon = newSelection.GetComponent<CharacterIcon>();
            if (!newSelectedIcon) {
                // Debug.LogError("CharacterIcon component not found on new cursor selectable");
                return;
            }

            if (selectedIcon) selectedIcon.SetSelected(isPlayer1, false);
            newSelectedIcon.SetSelected(isPlayer1, true);

            SoundManager.Instance.PlaySound(switchSFX, 2.5f);

            selectedIcon = newSelectedIcon;

            portrait.sprite = selectedBattler.sprite;
            nameText.text = selectedBattler.displayName;

            if (selectedBattler.passiveAbilityEffect == Battle.Battler.PassiveAbilityEffect.None && selectedBattler.activeAbilityEffect == Battle.Battler.ActiveAbilityEffect.None) {
                abilityText.text = "No special abilities";
            } else {
                if (selectedBattler.activeAbilityEffect == Battle.Battler.ActiveAbilityEffect.None) {
                    abilityText.text = selectedBattler.passiveAbilityDesc;
                } else {
                    abilityText.text = selectedBattler.passiveAbilityDesc 
                    + "\n\n" 
                    + "<b>"+selectedBattler.activeAbilityName+"</b>: "
                    + selectedBattler.activeAbilityDesc;
                }
            }
        }

        void OnValidate() {
            transitionHandler = GameObject.FindObjectOfType<TransitionScript>();
        }
    }
}