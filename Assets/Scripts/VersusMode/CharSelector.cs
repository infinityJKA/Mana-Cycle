using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using Sound;

namespace VersusMode {
    /// <summary>
    ///     Controls the box to the left/right of the character icon grid in the character select menu. 
    ///     This selector also controls the cursor on the character select grid through this script.
    /// </summary>
    public class CharSelector : MonoBehaviour {
        /// The CharSelect menu in this scene.
        [SerializeField] public CharSelectMenu menu;

        ///<summary>True for player 1, false for player 2.</summary>
        [SerializeField] private bool isPlayer1;

        ///<summary>Other player's charselector. Only used in mirroring battle preferences</summary>
        [SerializeField] public CharSelector opponentSelector;

        ///<summary>Input script used to move the cursor and select character</summary>
        [SerializeField] private InputScript inputScript;
        // set as the inputScript when in solo mode
        [SerializeField] private InputScript soloInputScript;

        [SerializeField] private Image background;
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;

        [SerializeField] private GameObject cpuLevelObject;
        [SerializeField] private TextMeshProUGUI cpuLevelText;
        [SerializeField] private GameObject cpuLevelLeftArrow, cpuLevelRightArrow;

        ///<summary>SFX played when interacting with menu</summary>
        [SerializeField] private GameObject switchSFX, noswitchSFX, selectSFX, unselectSFX, infoOpenSFX, infoCloseSFX, settingsToggleSFX, connectSFX;

        /// Fade in/out speed for the ability info & settings box
        [SerializeField] private float fadeSpeed;
        /// Displacement of the ability info & settings box when fading in/out
        [SerializeField] private Vector2 fadeDisplacement;

        ///<summary>Canvas group for the ability info box</summary>
        [SerializeField] private CanvasGroup abilityInfoCanvasGroup;
        ///<summary>Text field within the ability description object that displays passive&active ability</summary>
        [SerializeField] private TextMeshProUGUI abilityText;

        ///<summary>Canvas group for the char select settings box</summary>
        [SerializeField] private CanvasGroup settingsCanvasGroup;

        ///<summary>Toggle that toggles the ghost piece specific to this battle. Copies settings value at start</summary>
        [SerializeField] private Toggle ghostPieceToggle;
        ///<summary>Toggle that toggles the ghost piece specific to this battle. Copies settings value at start</summary>
        [SerializeField] private Toggle abilityToggle;

        /// <summary>Selectable to control the selected life count</summary>
        [SerializeField] public Selectable livesSelectable;
        /// <summary>the arrows to show if life count can be increased or decreased</summary>
        [SerializeField] private GameObject livesLeftArrow, livesRightArrow;
        /// <summary>current selected life count value</summary>
        public int lives {get; private set;} = 1;
        ///
        [SerializeField] private TextMeshProUGUI livesText;

        /// tip text in the corner, p2 tip text gets hidden in solo
        [SerializeField] private VersusTipText tipText;

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

        ///<summary> If currently in CPU select mode. This will control CPU cursor instead of cpu cursor. (player vs. ai p2 only)
        public bool isCpuCursor;
        ///<summary> If active. Will only be inactive if this is CPU and player1 is currently selecting </summary>
        private bool active;
        public bool Active {
            get { return active; }
            set {
                active = value;

                if (active) {
                    portrait.enabled = true;
                    nameText.enabled = true;
                    if (selectedIcon) {
                        SetSelection(selectedIcon.selectable);
                        selectedIcon.cursorImage.color = Color.white;
                    }
                }
                // if not active (player vs. cpu only): dimmed cursor if p1, hide if p2
                else {
                    if (isPlayer1) {
                        selectedIcon.cursorImage.color = new Color(1f, 1f, 1f, 0.5f);
                    } else {
                        portrait.color = new Color(1f, 1f, 1f, 0.5f);
                        nameText.enabled = false;
                        HideSelection();
                    }
                }

                if (menu.Mobile) {
                    if (isCpuCursor) {
                        if (active) {
                            cpuLevelObject.SetActive(true);
                            RefreshCpuLevel();
                        }
                    }
                }
            }
        }

        // when hovering over Random battler, current delay before shown battler changes
        private float randomChangeDelay;
        // current battler being shown as randomly selected fighter
        private Battle.Battler randomBattler;

        // CPU difficulty selected by the player - scale of 1-10
        private int cpuLevel;
        public int CpuLevel {
            get { return cpuLevel; }
            set {
                cpuLevel = value;
                if (active) RefreshCpuLevel();
            }
        }

        // only used in AI vs. AI mode - if this is P1 and wll give control to p2 after selecting cpu difficulty right now if true
        private bool selectingCpuLevel;

        // Spectrum of colors to tint the CPU number with increasing difficulty
        [SerializeField] private Color[] cpuLevelSpectrum;

        // cached on validate
        private TransitionScript transitionHandler;

        static readonly int minCpuLevel = 1, maxCpuLevel = 10;

        // properties
        public Battle.Battler selectedBattler { 
            get { 
                if (selectedIcon.battler.displayName == "Random" && randomBattler) {
                    return randomBattler;
                } else {
                    return selectedIcon.battler;
                }
            }
        }

        // whether or not to use the old smelly input scripts and not the new action input system
        [SerializeField] public bool useInputScripts = false;

        [SerializeField] private GameObject settingsInputModeButton;

        [SerializeField] private TextMeshProUGUI settingsInputModeButtonLabel;


        // If there is an input connected to this selector.
        // Only for Player vs Player. In all other modes this will always be true
        public bool connected {get; private set;} = true;
        [SerializeField] private Color disconnectBkgdColor, connectBkgdColor;

        [SerializeField] private GameObject connectTipLabel;

        // If player JUST connected this frame. will not register inputs for the button they pressed to join in charselect
        private bool connectedThisUpdate;

        private Vector2 centerPosition;
        void Start() {
            // TEMP FOR TESTING !! ,`:)
            // Storage.gamemode = Storage.GameMode.Solo;
            if (!menu.Mobile) {
                abilityInfoCanvasGroup.alpha = 0;
                centerPosition = abilityInfoCanvasGroup.transform.localPosition;
            }

            tipText.gameObject.SetActive(!menu.Mobile);
            gameObject.SetActive(true);
            transitionHandler = GameObject.FindObjectOfType<TransitionScript>();
        }

        void Update() {
            connectedThisUpdate = false;

            if (!enabled || !selectedIcon) return;

            if (abilityInfoAnimating) {
                // fade the ability window in/out according to state
                float abilityTarget = abilityInfoDisplayed ? 1 : 0;
                abilityInfoFadeAmount = Mathf.MoveTowards(abilityInfoFadeAmount, abilityTarget, fadeSpeed*Time.smoothDeltaTime);
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
                settingsFadeAmount = Mathf.MoveTowards(settingsFadeAmount, settingsTarget, fadeSpeed*Time.smoothDeltaTime);
                if (settingsFadeAmount == settingsTarget) settingsAnimating = false;
                settingsCanvasGroup.alpha = settingsFadeAmount;
                if (settingsAnimating) {
                    settingsCanvasGroup.transform.localPosition = centerPosition + (1-settingsFadeAmount) * fadeDisplacement * (isPlayer1?1:-1);
                } else {
                    settingsCanvasGroup.transform.localPosition = centerPosition;
                }
            }

            if (selectedIcon.battler.displayName == "Random") {
                if (randomChangeDelay <= 0 && (!lockedIn || !randomBattler)) {
                    randomChangeDelay = 0.125f;
                    var prevBattler = randomBattler;
                    while (!randomBattler || randomBattler.displayName == "Random" || randomBattler == prevBattler) {
                        randomBattler = battlerGrid.transform.GetChild(Random.Range(0, battlerGrid.transform.childCount-1)).GetComponent<CharacterIcon>().battler;
                    }

                    portrait.sprite = selectedBattler.sprite;
                    // nameText.text = selectedBattler.displayName;

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
                } else {
                    randomChangeDelay -= Time.smoothDeltaTime;
                }
            }

            // don't accept input if not active or old input scripts disabled
            if (!Active || !useInputScripts) return;

            if (settingsDisplayed) {
                // Look for a new icon to select in inputted directions, select if found
                if (Input.GetKeyDown(inputScript.Left)) SettingsCursorLeft();
                else if (Input.GetKeyDown(inputScript.Right)) SettingsCursorRight();
                else if (Input.GetKeyDown(inputScript.Up)) SetSettingsSelection(settingsSelection.FindSelectableOnUp());
                else if (Input.GetKeyDown(inputScript.Down)) SetSettingsSelection(settingsSelection.FindSelectableOnDown());
            }

            // if cpu, adjust cpu level while locked in
            else if (isCpuCursor && lockedIn) {
                if (Input.GetKeyDown(inputScript.Left)) {
                    if (CpuLevel > minCpuLevel) { CpuLevel--; RefreshCpuLevel(); }
                }
                else if (Input.GetKeyDown(inputScript.Right)) {
                    if (CpuLevel < maxCpuLevel) { CpuLevel++; RefreshCpuLevel(); }
                }
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
            
            if (Input.GetKeyDown(inputScript.Cast)) {
                OnCast();
            }

            if (Input.GetKeyDown(inputScript.Pause)) 
            {
                Back();
            }

            // show/hide ability info when rotate CCW is pressed
            if (Input.GetKeyDown(inputScript.RotateCCW))
            {
                if (!menu.Mobile) ToggleAbilityInfo();
            }
            if (Input.GetKeyDown(inputScript.RotateCW))
            {
                if (!menu.Mobile) ToggleSettings();
            }
        }

        // for use with CharSelectorController

        public void OnMoveLeft() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;
            if (settingsDisplayed) SettingsCursorLeft();
            else if (isCpuCursor && lockedIn) {
                if (CpuLevel > minCpuLevel) { CpuLevel--; RefreshCpuLevel(); }
            }
            else if (!lockedIn) SetSelection(selectedIcon.selectable.FindSelectableOnLeft());
            
        }

        public void OnMoveRight() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;
            if (settingsDisplayed) SettingsCursorRight();
            else if (isCpuCursor && lockedIn) {
                if (CpuLevel < maxCpuLevel) { CpuLevel++; RefreshCpuLevel(); }
            }
            else if (!lockedIn) SetSelection(selectedIcon.selectable.FindSelectableOnRight());
        }

        public void OnMoveUp() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;
            if (settingsDisplayed) SetSettingsSelection(settingsSelection.FindSelectableOnUp());
            else if (!lockedIn) SetSelection(selectedIcon.selectable.FindSelectableOnUp());
        }

        public void OnMoveDown() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;
            if (settingsDisplayed) SetSettingsSelection(settingsSelection.FindSelectableOnDown());
            else if (!lockedIn) SetSelection(selectedIcon.selectable.FindSelectableOnDown());
        }

        public void OnCast() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;

            // when in settings menu, cast will toggle the current toggle, press the current button, etc..
            if (settingsDisplayed) {
                var toggle = settingsSelection.GetComponent<Toggle>();
                if (toggle) {
                    toggle.isOn = !toggle.isOn;
                    Instantiate(settingsToggleSFX);
                }

                var button = settingsSelection.GetComponent<Button>();
                if (button) {
                    button.onClick.Invoke();
                    Instantiate(settingsToggleSFX);
                }

                // if any battle preferences were just toggled, update its state in Storage and the other player's settings toggle
                if (settingsSelection == abilityToggle) {
                    PlayerPrefs.SetInt("enableAbilities", abilityToggle.isOn ? 1 : 0);
                    opponentSelector.abilityToggle.isOn = abilityToggle.isOn;
                }
            }

            // if this is CPU and locked into selecting CPU level, give control to other board
            else if (selectingCpuLevel && !menu.Mobile) {
                selectingCpuLevel = false;
                Instantiate(selectSFX);
                Active = false;
                opponentSelector.Active = true;
                RefreshLockVisuals();
            }
            // otherwise, lock/unlock in this character
            else {
                if (menu.IsBothPlayersReady()) {
                    menu.StartIfReady();
                } else {
                    ToggleLock();
                }
            }
        }

        public void OnBack() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;
            Back();
        }

        public void OnAbilityInfo() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;
            if (!menu.Mobile) ToggleAbilityInfo();
        }

        public void OnSettings() {
            if (!enabled || !selectedIcon || connectedThisUpdate) return;
            if (!menu.Mobile) ToggleSettings();
        }

        // initialization :(
        public void MenuInit() {
            RefreshLockVisuals();

            // Set cpu cursor to true if in Versus: player vs. opponent only. set to cpu cursor and false if this is p2
            if (Storage.gamemode == Storage.GameMode.Versus && !Storage.isPlayerControlled2 && Storage.level == null) {
                if (!isPlayer1) {
                    isCpuCursor = true;
                    Active = false;
                    portrait.enabled = false;
                } else {
                    if (!Storage.isPlayerControlled1) isCpuCursor = true;
                    // inputs should be solo inputs scripts, as the player will play in the game
                    inputScript = soloInputScript;
                    opponentSelector.inputScript = soloInputScript;
                    tipText.SetInputs(inputScript);
                    Active = true;
                }
            } else {
                Active = true;
            }
            
            if (Storage.gamemode == Storage.GameMode.Solo || (isPlayer1 && (!Storage.isPlayerControlled2 && Storage.level != null)))
            {
                // set solo mode inputs 
                // TODO change tip text depending on inputs
                inputScript = soloInputScript;
                tipText.SetInputs(inputScript);

                // hide p2 elements in in solo mode
                if (!isPlayer1)
                {
                    tipText.gameObject.SetActive(false);
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

            if (menu.Mobile && isCpuCursor && Active) {
                cpuLevelObject.SetActive(true);
                RefreshCpuLevel();
            } else {
                cpuLevelObject.SetActive(false);
            }

            if (!Storage.isPlayerControlled2) {
                settingsInputModeButton.gameObject.SetActive(false);
            }

            SetSettingsSelection(ghostPieceToggle);
            ghostPieceToggle.isOn = PlayerPrefs.GetInt("drawGhostPiece", 1) == 1;
            abilityToggle.isOn = PlayerPrefs.GetInt("enableAbilities", 1) == 1;
        }

        public void ToggleLock()
        {
            lockedIn = !lockedIn;  

            if (isPlayer1 && isCpuCursor && !selectingCpuLevel && !menu.Mobile) {
                Instantiate(settingsToggleSFX);
            } else {
                Instantiate(lockedIn ? selectSFX : unselectSFX);
            }

            // when locking in, disable and enable cpu selector
            if (lockedIn && opponentSelector.isCpuCursor) {
                // is this is also a cpu cursor (AI vs AI)
                if (isPlayer1) {
                    if (isCpuCursor && !selectingCpuLevel && !menu.Mobile) {
                        selectingCpuLevel = true;
                    } else {
                        Active = false;
                        opponentSelector.Active = true;
                    }
                }
            }

            if (!menu.Mobile) {
                if (isCpuCursor) {
                    Debug.Log("toggling cpu level obj");
                    cpuLevelObject.SetActive(lockedIn);
                } else {
                    cpuLevelObject.SetActive(menu.Mobile && isCpuCursor);
                }
                if (cpuLevelObject.activeInHierarchy) RefreshCpuLevel();
            }

            RefreshLockVisuals();
            menu.RefreshStartButton();
        }

        /// <summary>Is run when the pause button is pressed while controlled
        /// Also called when back button is pressed</summary>
        public void Back() {
            // close ability info/settings menu if it is open
            Debug.Log("uwef" + lockedIn);
            if (!menu.Mobile && abilityInfoDisplayed) ToggleAbilityInfo();
            else if (!menu.Mobile && settingsDisplayed) ToggleSettings();

            // stop selecting cpu level if selecting
            else if (selectingCpuLevel && !menu.Mobile) {
                selectingCpuLevel = false;
                RefreshLockVisuals();
            }
            
            // unlock in when pause pressed
            else if (lockedIn) ToggleLock();

            // if this is the CPU cursor (player vs cpu only), return active state to the player and re-disable this
            else if (isCpuCursor && !isPlayer1) {
                Active = false;
                opponentSelector.Active = true;
                if (Storage.isPlayerControlled1) {
                    selectedIcon.SetCPUHovered(true, true);
                } else {
                    selectedIcon.SetSelected(isPlayer1, true, true);
                }
            }

            // or go back to menu if not locked in
            else {
                ReturnToMenu();
            }
        }

        public void ReturnToMenu() {
            if (!transitionHandler) {
                Debug.LogError("Transition handler not found in scene!");
                return;
            }
            if (Storage.gamemode != Storage.GameMode.Solo) {
                transitionHandler.WipeToScene("MainMenu", reverse: true);
            } else {
                transitionHandler.WipeToScene("SoloMenu", reverse: true);
            }
        }

        void RefreshLockVisuals() {
            if (!connected || !selectedIcon) return;
            if (lockedIn){
                portrait.color = new Color(1.0f, 1.0f, 1.0f, (selectingCpuLevel && !menu.Mobile) ? 0.65f : 1f);
                nameText.text = selectedBattler.displayName;
                nameText.fontStyle = TMPro.FontStyles.Bold;
            }
            else {
                portrait.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                nameText.fontStyle = TMPro.FontStyles.Normal;
                nameText.text = (selectedIcon.battler.displayName == "Random") ? "Random" : selectedBattler.displayName;
            }
        }

        public void RefreshCpuLevel() {
            cpuLevelText.text = CpuLevel+"";
            cpuLevelText.color = cpuLevelSpectrum[CpuLevel];
            cpuLevelLeftArrow.SetActive(CpuLevel > minCpuLevel);
            cpuLevelRightArrow.SetActive(CpuLevel < maxCpuLevel);
        }

        void ToggleAbilityInfo() {
            if (settingsDisplayed) ToggleSettings();
            abilityInfoDisplayed = !abilityInfoDisplayed;
            abilityInfoAnimating = true;
            Instantiate(abilityInfoDisplayed ? infoOpenSFX : infoCloseSFX);
        }

        void ToggleSettings() {
            if (abilityInfoDisplayed) ToggleAbilityInfo();
            settingsDisplayed = !settingsDisplayed;
            settingsAnimating = true;
            // selectedIcon.cursorImage.color = new Color(1f, 1f, 1f, settingsDisplayed ? 0.5f : 1f);
            Instantiate(settingsDisplayed ? infoOpenSFX : infoCloseSFX);
        }

        static int minLives = 1, maxLives = 15;
        public void AdjustLives(int delta) {
            lives = Mathf.Clamp(lives+delta, minLives, maxLives);
            opponentSelector.lives = lives;
            RefreshLives();
            opponentSelector.RefreshLives();
        }

        public void SetLives(int lives) {
            this.lives = lives;
            AdjustLives(0);
        }

        void RefreshLives() {
            livesText.text = lives+"";
            livesLeftArrow.SetActive(lives > minLives);
            livesRightArrow.SetActive(lives < maxLives);
        }

        public void AdjustCPULevel(int delta) {
            CpuLevel = Mathf.Clamp(CpuLevel+delta, minCpuLevel, maxCpuLevel);
            // RefreshCpuLevel(); // called in CpuLevel property
        }

        void SettingsCursorLeft() {
            if (settingsSelection == livesSelectable) {
                AdjustLives(-1);
            } else {
                SetSettingsSelection(settingsSelection.FindSelectableOnLeft());
            }
        }

        void SettingsCursorRight() {
            if (settingsSelection == livesSelectable) {
                AdjustLives(1);
            } else {
                SetSettingsSelection(settingsSelection.FindSelectableOnRight());
            }
        }

        public void SetSettingsSelection(Selectable selectable) {
            if (selectable == null || selectable == settingsSelection) return;
            if (settingsSelection) settingsSelection.OnDeselect(null);
            settingsSelection = selectable;
            settingsSelection.OnSelect(null);
        }

        public void SetSelection(Selectable newSelection) {
            if (!newSelection) {
                if (Application.isPlaying) Instantiate(noswitchSFX);
                return;
            }

            CharacterIcon newSelectedIcon = newSelection.GetComponent<CharacterIcon>();
            if (!newSelectedIcon) {
                // Debug.LogError("CharacterIcon component not found on new cursor selectable");
                return;
            }

            if (isCpuCursor) {
                if (!Storage.isPlayerControlled1) {
                    if (selectedIcon) selectedIcon.SetSelected(isPlayer1, false);
                    newSelectedIcon.SetSelected(isPlayer1, true);
                } else {
                    if (selectedIcon) selectedIcon.SetCPUHovered(false);
                    newSelectedIcon.SetCPUHovered(true);
                }
            } else {
                if (selectedIcon) selectedIcon.SetSelected(isPlayer1, false);
                newSelectedIcon.SetSelected(isPlayer1, true);
            }

            Instantiate(switchSFX);

            selectedIcon = newSelectedIcon;

            portrait.sprite = selectedBattler.sprite;
            nameText.text = (selectedIcon.battler.displayName == "Random") ? "Random" : selectedBattler.displayName;

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

        public void HideSelection() {
            if (selectedIcon) selectedIcon.SetSelected(isPlayer1, false);
        }

        public void DualKeyboardEnabled() {
            useInputScripts = true;
            settingsInputModeButtonLabel.text = "Switch to Multi-Device Inputs";
            if (settingsDisplayed) ToggleSettings();
        }

        public void DualKeyboardDisabled() {
            useInputScripts = false;
            settingsInputModeButtonLabel.text = "Switch to Dual-Keyboard Inputs";
            if (settingsDisplayed) ToggleSettings();
        }

        // used by CharSelectMenu to receive player pereference decisions
        public bool doGhostPiece { 
            get { return ghostPieceToggle.isOn; }
            set { ghostPieceToggle.isOn = value; }    
        }

        public bool enableAbilities { 
            get { return abilityToggle.isOn; } 
        }

        public void Disconnect() {
            Debug.Log(name + " disconnected");
            if (lockedIn) ToggleLock();
            connected = false;
            connectTipLabel.SetActive(true);
            background.color = disconnectBkgdColor;
            HideSelection();
            portrait.gameObject.SetActive(false);
            nameText.gameObject.SetActive(false);
        }

        public void Connect() {
            Debug.Log(name + " connected");
            connected = true;
            connectTipLabel.SetActive(false);
            background.color = connectBkgdColor;
            if (selectedIcon != null) selectedIcon.SetSelected(isPlayer1, true);
            portrait.gameObject.SetActive(true);
            nameText.gameObject.SetActive(true);
            connectedThisUpdate = true;
            Instantiate(connectSFX);
            RefreshLockVisuals();
        }
    }
}