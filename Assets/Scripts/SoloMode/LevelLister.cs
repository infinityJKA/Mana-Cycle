using UnityEngine;

using TMPro;

using ConvoSystem;
using Sound;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace SoloMode {
    public class LevelLister : MonoBehaviour
    {
        /** Conversation handler, to run conversation when level is selected */
        [SerializeField] private ConvoHandler convoHandler;

        // List level object caches
        /** list text component where the lines are written out */
        // level container; height is controlled for mobile level lister scroll thing
        [SerializeField] private RectTransform levelContainerTransform;
        [SerializeField] private RectTransform listTransform;
        [SerializeField] private RectTransform tabTransform;

        // Array of the 4(+) tab texts
        private TextMeshProUGUI[] tabTexts;
        // Target colors for all tab texts
        private Color[] tabColors, tabTargetColors;
        // Color lerp positions for all tab texts
        private float[] tabColorPositions;

        // self explanatory
        [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

        [SerializeField] private GameObject highScoreBG;
        [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

        /** Inputs that control the level list */
        [SerializeField] private InputScript[] inputScripts;

        /** Initial offset of the list, saved on start from anchored position */
        protected Vector2 listOffset;
        private Vector2 tabOffset;

        [SerializeField] protected float levelYSpacing;
        [SerializeField] private float tabScrollAmount;

        /** Index of current level selected */
        public int[] selectedLevelIndexes;
        public int selectedTabIndex;
        /** Descent between lines in the level list */
        // private float decLine;

        /** current targeted scroll position */
        protected Vector2 targetListPosition = Vector2.left*50f;
        private Vector2 targetTabPosition;
        /** current scroll velocity */
        private Vector2 vel = Vector2.zero;
        private Vector2 vel2 = Vector2.zero;

        /** If the level list is currently focused, instead of dialogue */
        private bool focused; 

        // menu sfx
        [SerializeField] private AudioClip moveSFX;
        [SerializeField] private AudioClip swapTabSFX;
        [SerializeField] private AudioClip selectSFX;
        [SerializeField] private AudioClip errorSFX;

        [SerializeField] protected SoloMenuTab[] tabs;

        // Transform of all pre-computed lists of levels, computed on start
        [SerializeField] protected Transform levelTabTransform;
        // Prefab containing a listed level - default text is a flavor line
        [SerializeField] private GameObject listedLevelPrefab;
        // Prefab for the container of all the listed levels in a tab
        [SerializeField] private GameObject tabLevelsPrefab;
        // Prefab containing the name of a tab
        [SerializeField] private GameObject tabNamePrefab;
        // Used in mobile mode. Tab is centered instead of hugging the edge of the solo level menu
        [SerializeField] private bool centerTab;
        // If the target color of tabs should be animated instead of instantly set.
        [SerializeField] private bool animateTabColors;
        
        // colors used for displayed levels
        [SerializeField] private Color levelColor, lockedColor, selectedColor, clearedColor, tabColor, tabSelectedColor;

        // Curve for the color change of tabs
        [SerializeField] private AnimationCurve tabColorAnimationCurve;
        // color fade speed of tab color
        [SerializeField] private float tabFadeSpeed = 10f;

        // If the level list should smooth damp to align to top - shoould be turned off in mobile
        [SerializeField] private bool autoMoveLevelList;

        // If description box should be shown and updated - false if mobile mode
        [SerializeField] protected bool showDescription = true;

        // If the cursor should be displayed. Will be automatically set to true if left/right arrow keypress is detected.
        // Set to false initially in mobile but if keyboard is connected or sum then show the arrows
        [SerializeField] private bool showLevelCursor = true;

        // True if buttons should open the corresponding level when clicked. (mobile only)
        // eventually, should open the description box for that level with a play and exit button, but ill work on that later
        [SerializeField] private bool levelsClickable = false;

        // If this should lead to mobile scenes or not.
        [SerializeField] private bool mobile;

        // currently unused references
        [SerializeField] private GameObject upArrow;
        [SerializeField] private GameObject downArrow;

        


        [SerializeField] int tabWhitespacing = 4;
        [SerializeField] int flavorLineCount = 15;

        // Start is called before the first frame update
        void Start()
        {
            selectedLevelIndexes = new int[tabs.Length];
            selectedTabIndex = 0;
            focused = true;

            // generates all the listed levels and organizes them into a transform
            MakeTabLevelLists();

            // initialize offsets for position animation
            listOffset = listTransform.anchoredPosition;
            tabOffset = tabTransform.anchoredPosition;

            // refresh shown information for first selected current tab & level
            tabTexts[0].color = tabSelectedColor;

            RefreshTab();
            RefreshCursor();
            RefreshDescription();
        }

        // Update is called once per frame
        void Update()
        {
            if (!focused) return;

            foreach (InputScript inputScript in inputScripts) {

                if (Input.GetKeyDown(inputScript.Up))
                {
                    MoveCursor(-1);
                    SoundManager.Instance.PlaySound(moveSFX, pitch : 1.18f);
                    break;
                }

                if (Input.GetKeyDown(inputScript.Down))
                {
                    MoveCursor(1);
                    SoundManager.Instance.PlaySound(moveSFX, pitch : 1.06f);
                    break;
                }

                if (Input.GetKeyDown(inputScript.Left))
                {
                    LeftTabArrow();
                    break;
                }

                if (Input.GetKeyDown(inputScript.Right))
                {
                    RightTabArrow();
                    break;
                }

                // pause - go back to main menu
                if (Input.GetKeyDown(inputScript.Pause))
                {
                    Back();
                    break;
                }

                // cast - open selected level
                if (Input.GetKeyDown(inputScript.Cast))
                {
                    ConfirmLevel(selectedLevel);
                    break;
                }
            }

            // smoothly update displayed y position of level list
            if (autoMoveLevelList) listTransform.anchoredPosition = 
            Vector2.SmoothDamp(listTransform.anchoredPosition, targetListPosition, ref vel, 0.1f);

            tabTransform.anchoredPosition = 
            Vector2.SmoothDamp(tabTransform.anchoredPosition, targetTabPosition, ref vel2, 0.1f);

            // animate tab colors
            if (animateTabColors) {
                for (int i=0; i<tabs.Length; i++) {
                    if (tabColorPositions[i] < 1f) {
                        tabColorPositions[i] += Time.deltaTime*tabFadeSpeed;
                        float colorT = Mathf.Min(1f, tabColorAnimationCurve.Evaluate(tabColorPositions[i]));
                        tabTexts[i].color = Color.Lerp(tabColors[i], tabTargetColors[i], colorT);
                    }
                }
            }
        }

        public void Back() {
            StoreSelections();
            GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("MainMenu", reverse : true);
        }

        public void ConfirmLevel(Level pressedLevel) {
            if (!pressedLevel.RequirementsMet()) 
            {
                SoundManager.Instance.PlaySound(errorSFX);
                return;
            }
            SoundManager.Instance.PlaySound(selectSFX);

            StoreSelections();
            Storage.level = pressedLevel; 
            Storage.lives = Storage.level.lives;
            Storage.gamemode = Storage.GameMode.Solo;
            convoHandler.StartLevel(pressedLevel);
            focused = false;
            Storage.levelSelectedThisInput = true;

            gameObject.SetActive(false);
        }

        public void LeftTabArrow() {
            MoveTabCursor(-1);
            SoundManager.Instance.PlaySound(swapTabSFX, pitch : 1.68f);
        }

        public void RightTabArrow() {
            MoveTabCursor(1);
            SoundManager.Instance.PlaySound(swapTabSFX, pitch : 1.56f);
        }

        /// <summary>
        /// Constructs the gameObjects that hold all the levels, and a text object for all tabs.
        /// </summary>
        public void MakeTabLevelLists() {
            selectedLevelIndexes = new int[tabs.Length];

            // make string for tab list - might break up later for color + not performance tanks
            // string newTabText = "";
            // for (int i = 0; i < tabs.Length; i++)
            // {
            //     SoloMenuTab currentTab = tabs[i];
            //     newTabText += currentTab.tabName + new string(' ', tabWhitespacing);
            // }
            // tabText.text = newTabText;

            // Generate each tab
            foreach (Transform child in tabTransform) Destroy(child.gameObject);
            foreach (Transform child in levelTabTransform) Destroy(child.gameObject);

            tabTexts = new TextMeshProUGUI[tabs.Length];
            if (animateTabColors) {
                tabColors = new Color[tabs.Length];
                tabTargetColors = new Color[tabs.Length];
                tabColorPositions = new float[tabs.Length];
            }

            Vector2 tabOffset = Vector2.zero;
            for (int t=0; t<tabs.Length; t++) {
                SoloMenuTab tab = tabs[t];

                var tabName = Instantiate(tabNamePrefab, tabTransform).GetComponent<TextMeshProUGUI>();
                tabTexts[t] = tabName;
                if (animateTabColors) {
                    tabColors[t] = tabColor;
                    tabTargetColors[t] = tabColor;
                    tabTexts[t].color = tabColor;
                    tabColorPositions[t] = 1f;
                }
                tabName.text = tab.tabName;
                tabName.rectTransform.anchoredPosition = tabOffset;
                int len = centerTab ? 6 : tab.tabName.Length;
                tabOffset += Vector2.right * tabScrollAmount * (len + tabWhitespacing);

                // Create the empty gameobject that wil house all the listed levels
                GameObject tabLevelsObject = Instantiate(tabLevelsPrefab);
                tabLevelsObject.transform.SetParent(levelTabTransform, false);

                Vector2 offset = Vector2.up*levelYSpacing*flavorLineCount;

                MakeFlavorLines(tabLevelsObject.transform, ref offset);

                // Add a listed level element for each level in the tab
                for (int i = 0; i < tab.levelsList.Length; i++) {
                    Level level = tab.levelsList[i];

                    var listedLevel = Instantiate(listedLevelPrefab, tabLevelsObject.transform);
                    listedLevel.transform.localPosition = offset;

                    bool selected = i == selectedLevelIndexes[selectedTabIndex];
                    RefreshListedLevelText(level, listedLevel.GetComponent<TextMeshProUGUI>(), selected);

                    if (levelsClickable) {
                        var button = listedLevel.gameObject.GetComponent<UnityEngine.UI.Button>();
                        button.onClick.AddListener(() => ConfirmLevel(level));
                        if (!level.RequirementsMet()) button.interactable = false;
                    }

                    offset += Vector2.down*levelYSpacing;
                }

                MakeFlavorLines(tabLevelsObject.transform, ref offset);

                // initially hide all tabs other than first tab
                if (t>0) tabLevelsObject.SetActive(false);
            }

            if (levelContainerTransform && !autoMoveLevelList) levelContainerTransform.sizeDelta 
            = new Vector2(levelContainerTransform.sizeDelta.x, (selectedTab.levelsList.Length-1)*levelYSpacing);

            RefreshDescription();
        }

        protected void RefreshListedLevelText(Level level, TextMeshProUGUI listedLevel, bool selected) {
            if (!levelsClickable) listedLevel.color = level.RequirementsMet() ? ((selected) ? selectedColor : levelColor) : lockedColor; // i love nested ternary statements
            listedLevel.text = level.levelName + ((selected && showLevelCursor) ? " <" : "") + (level.IsCleared() ? "  <color=#00ffdf>X" : "  <color=#00000000>X");
        }

        void MakeFlavorLines(Transform parent, ref Vector2 offset) {
            for (int i=0; i<flavorLineCount; i++) {
                var flavorLine = Instantiate(listedLevelPrefab, parent).GetComponent<TextMeshProUGUI>();
                if (levelsClickable) flavorLine.gameObject.GetComponent<UnityEngine.UI.Button>().enabled = false;
                flavorLine.color = levelColor;
                flavorLine.rectTransform.localPosition = offset;
                offset += Vector2.down*levelYSpacing;
            }
        }

        protected SoloMenuTab selectedTab { get { return tabs[selectedTabIndex]; } }
        protected Level selectedLevel { get { return selectedTab.levelsList[selectedLevelIndexes[selectedTabIndex]];} }

        private void StoreSelections()
        {
            Storage.lastTabSelectedIndex = selectedTabIndex;
            Storage.lastLevelSelectedIndex = selectedLevelIndexes[selectedTabIndex];
        }


        Transform listedLevelsTransform { get { return levelTabTransform.GetChild( selectedTabIndex ); } }
        // Transform listedLevelTransform { get { return listedLevelTransform.GetChild( selectedLevelIndex ); } }
        protected TextMeshProUGUI selectedText { get { return listedLevelsTransform.GetChild(flavorLineCount+selectedLevelIndex).GetComponent<TextMeshProUGUI>();} }

        protected int selectedLevelIndex { 
            get { return selectedLevelIndexes[selectedTabIndex]; }
            set {
                selectedLevelIndexes[selectedTabIndex] = value;
            }    
        }

        void MoveCursor(int delta)
        {
            if (!showLevelCursor) return;

            // don't move cursor if movement will send cursor outside the list
            if (selectedLevelIndex+delta < 0 || selectedLevelIndex+delta >= selectedTab.levelsList.Length) return;

            RefreshListedLevelText(selectedLevel, selectedText, false);
            selectedLevelIndexes[selectedTabIndex] += delta;
            RefreshListedLevelText(selectedLevel, selectedText, true);

            // update the level target position to animate to
            RefreshCursor();
        }

        void RefreshCursor() {
            // update the targeted level scroll position
            if (autoMoveLevelList) targetListPosition = listOffset + Vector2.up * selectedLevelIndex * levelYSpacing;

            // show the description of the newly hovered level
            RefreshDescription();
        }

        void MoveTabCursor(int delta) {
            // don't move cursor if movement will send cursor outside the list
            if (selectedTabIndex+delta < 0 || selectedTabIndex+delta >= tabs.Length) return;

            levelTabTransform.GetChild(selectedTabIndex).gameObject.SetActive(false);
            if (animateTabColors) {
                tabColors[selectedTabIndex] = tabTexts[selectedTabIndex].color;
                tabColorPositions[selectedTabIndex] = 0f;
                tabTargetColors[selectedTabIndex] = tabColor;
            } else {
                tabTexts[selectedTabIndex].color = tabColor;
            }

            selectedTabIndex += delta;

            levelTabTransform.GetChild(selectedTabIndex).gameObject.SetActive(true);
            if (animateTabColors) {
                tabColors[selectedTabIndex] = tabTexts[selectedTabIndex].color;
                tabColorPositions[selectedTabIndex] = 0f;
                tabTargetColors[selectedTabIndex] = tabSelectedColor;
            } else {
                tabTexts[selectedTabIndex].color = tabSelectedColor;
            }

            if (levelContainerTransform && !autoMoveLevelList) levelContainerTransform.sizeDelta 
            = new Vector2(levelContainerTransform.sizeDelta.x, (selectedTab.levelsList.Length-1)*levelYSpacing);

            RefreshTab();
            RefreshListedLevelText(selectedLevel, selectedText, true);
            RefreshCursor();
        }

        void RefreshDescription()
        {
            if (!showDescription) return;

            // display the description and time of the selected level
            descriptionText.text = selectedLevel.description;

            bool selectedCleared = selectedLevel.IsCleared();
            highScoreBG.SetActive(selectedCleared);
            highScoreText.text = "High Score: "+selectedLevel.GetHighScore();

            // if level series, show length instead of time
            if (selectedLevel.nextSeriesLevel == null)
            {
                timeText.text = "Time: " + ((selectedLevel.time != -1) ? Utils.FormatTime(selectedLevel.time): "∞");
            }
            else 
            {
                timeText.text = (selectedLevel.GetAheadCount() + 1) + " Matches";
            }
        }

        void RefreshTab() {
            // make tab list string
            // string newTabText = "";
            // for (int i = 0; i < tabs.Length; i++)
            // {
            //     // most readable code in ohio
            //     newTabText += (selectedTabIndex == i) ? "<color=white>" : "<color=#10FF10>";
            //     newTabText += (selectedTabIndex == i && selectedTabIndex > 0) ? "< " : "  ";

            //     SoloMenuTab currentTab = tabs[i];
            //     newTabText += currentTab.tabName;

            //     newTabText += (selectedTabIndex == i && selectedTabIndex < tabs.Length-1) ? " ></color>" : "  </color>";
            // }

            // tabText.text = newTabText;

            // update target tab scroll pos
            float newTabPos = 0;
            for (int i = 0; i < selectedTabIndex; i++)
            {
                int len = centerTab ? 6 : tabs[i].tabName.Length;
                newTabPos += (len+tabWhitespacing) * tabScrollAmount;
            }
            targetTabPosition = tabOffset + Vector2.left * newTabPos;
            // Debug.Log("TABPOS: " + targetTabPosition);
        }

        public void SetFocus(bool f){
            focused = f;
        }

        // loop through each level and return the index of the first one that hasn't been cleard.
        public int GetNextLevel()
        {
            for (int i = 0; i < selectedTab.levelsList.Length; i++)
            {
                if (!(PlayerPrefs.GetInt(selectedTab.levelsList[i].levelName+"_Cleared", 0) == 1)) return i;
            }
            // if all levels are cleared, start at 0
            return 0;
        }
    }

    #if (UNITY_EDITOR)
    [CustomEditor(typeof(LevelLister))]
    public class LevelListEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Regenerate tab level objects")) {
                LevelLister thisScript = (LevelLister) target;
                thisScript.MakeTabLevelLists();
            }
        }
    }
    #endif
}