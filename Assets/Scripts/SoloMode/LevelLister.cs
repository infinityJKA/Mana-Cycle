using UnityEngine;
using System;

using TMPro;

using ConvoSystem;
using Sound;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace SoloMode {
    public class LevelLister : MonoBehaviour
    {
        // morgan please comment your code in the future :(

        /** Conversation handler, to run conversation when level is selected */
        [SerializeField] private ConvoHandler convoHandler;

        // List level object caches
        /** list text component where the lines are written out */
        [SerializeField] private RectTransform listTransform;
        [SerializeField] private TMPro.TextMeshProUGUI tabText;

        // self explanatory
        [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

        [SerializeField] private GameObject highScoreBG;
        [SerializeField] private TMPro.TextMeshProUGUI highScoreText;

        /** Inputs that control the level list */
        [SerializeField] private InputScript[] inputScripts;

        /** Initial offset of the list, saved on start from anchored position */
        private Vector2 listOffset;
        private Vector2 tabOffset;

        [SerializeField] private float levelScrollAmount;
        [SerializeField] private float tabScrollAmount;

        /** Index of current level selected */
        public int[] selectedLevelIndexes;
        public int selectedTabIndex;
        /** Descent between lines in the level list */
        // private float decLine;

        /** current targeted scroll position */
        private Vector2 targetListPosition;
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

        [SerializeField] private SoloMenuTab[] tabs;

        // Transform of all pre-computed lists of levels, computed on start
        [SerializeField] private Transform levelTabTransform;
        // Prefab containing a listed level - default text is a flavor line
        [SerializeField] private TextMeshProUGUI textLine;
        
        // colors used for displayed levels
        [SerializeField] private Color levelColor, lockedColor, selectedColor, clearedColor;

        // currently unused references
        [SerializeField] GameObject upArrow;
        [SerializeField] GameObject downArrow;

        


        private static int tabWhitespacing = 4;
        private static int flavorLineCount = 15;

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
            tabOffset = tabText.rectTransform.offsetMin;

            // refresh shown information for first selected current tab & level
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
                }

                if (Input.GetKeyDown(inputScript.Down))
                {
                    MoveCursor(1);
                    SoundManager.Instance.PlaySound(moveSFX, pitch : 1.06f);
                }

                if (Input.GetKeyDown(inputScript.Right))
                {
                    MoveTabCursor(1);
                    SoundManager.Instance.PlaySound(swapTabSFX, pitch : 1.56f);
                }

                if (Input.GetKeyDown(inputScript.Left))
                {
                    MoveTabCursor(-1);
                    SoundManager.Instance.PlaySound(swapTabSFX, pitch : 1.68f);
                }

                // pause - go back to main menu
                if (Input.GetKeyDown(inputScript.Pause))
                {
                    StoreSelections();
                    GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("MainMenu", reverse : true);
                }

                // cast - open selected level
                if (Input.GetKeyDown(inputScript.Cast))
                {
                    if (!selectedLevel.RequirementsMet()) 
                    {
                        SoundManager.Instance.PlaySound(errorSFX);
                        return;
                    }

                    StoreSelections();
                    Storage.level = selectedLevel; 
                    Storage.lives = Storage.level.lives;
                    Storage.gamemode = Storage.GameMode.Solo;
                    convoHandler.StartLevel(selectedLevel);
                    focused = false;
                    Storage.levelSelectedThisInput = true;
                    SoundManager.Instance.PlaySound(selectSFX);
                }
            }

            // smoothly update displayed y position of level list
            listTransform.anchoredPosition = 
            Vector2.SmoothDamp(listTransform.anchoredPosition, targetListPosition, ref vel, 0.1f);

            tabText.rectTransform.offsetMin = 
            Vector2.SmoothDamp(tabText.rectTransform.offsetMin, targetTabPosition, ref vel2, 0.1f);
        }

        public void MakeTabLevelLists() {
            foreach (Transform child in levelTabTransform) DestroyImmediate(child.gameObject);

            selectedLevelIndexes = new int[tabs.Length];

            // Generate each tab
            for (int t=0; t<tabs.Length; t++) {
                SoloMenuTab tab = tabs[t];

                // Create the empty gameobject that wil house all the listed levels
                RectTransform tabLevelsTransform = new GameObject(tab.name, typeof(RectTransform)).GetComponent<RectTransform>();
                tabLevelsTransform.SetParent(levelTabTransform, false);

                Vector2 offset = Vector2.up*levelScrollAmount*flavorLineCount;

                MakeFlavorLines(tabLevelsTransform, ref offset);

                // Add a listed level element for each level in the tab
                for (int i = 0; i < tab.levelsList.Length; i++) {
                    Level level = tab.levelsList[i];

                    var listedLevel = Instantiate<TextMeshProUGUI>(textLine, tabLevelsTransform);
                    listedLevel.rectTransform.localPosition = offset;

                    bool selected = i == selectedLevelIndexes[selectedTabIndex];
                    RefreshListedLevelText(level, listedLevel, selected);

                    offset += Vector2.down*levelScrollAmount;
                }

                MakeFlavorLines(tabLevelsTransform, ref offset);

                if (t>0) tabLevelsTransform.gameObject.SetActive(false);
            }

            RefreshDescription();
        }

        void RefreshListedLevelText(Level level, TextMeshProUGUI listedLevel, bool selected) {
            listedLevel.color = level.RequirementsMet() ? (selected ? selectedColor : levelColor) : lockedColor; // i love nested ternary statements
            listedLevel.text = level.levelName + (selected ? " <" : "") + (level.IsCleared() ? "  <color=#00ffdf>X" : "  <color=#000000>X");
        }

        void MakeFlavorLines(Transform parent, ref Vector2 offset) {
            for (int i=0; i<flavorLineCount; i++) {
                var flavorLine = Instantiate(textLine, parent);
                flavorLine.rectTransform.localPosition = offset;
                offset += Vector2.down*levelScrollAmount;
            }
        }

        SoloMenuTab selectedTab { get { return tabs[selectedTabIndex]; } }
        Level selectedLevel { get { return selectedTab.levelsList[selectedLevelIndexes[selectedTabIndex]];} }

        private void StoreSelections()
        {
            Storage.lastTabSelectedIndex = selectedTabIndex;
            Storage.lastLevelSelectedIndex = selectedLevelIndexes[selectedTabIndex];
        }


        Transform listedLevelsTransform { get { return levelTabTransform.GetChild( selectedTabIndex ); } }
        // Transform listedLevelTransform { get { return listedLevelTransform.GetChild( selectedLevelIndex ); } }
        TextMeshProUGUI selectedText { get { return listedLevelsTransform.GetChild(flavorLineCount+selectedLevelIndex).GetComponent<TextMeshProUGUI>();} }

        int selectedLevelIndex { 
            get { return selectedLevelIndexes[selectedTabIndex]; }
            set {
                selectedLevelIndexes[selectedTabIndex] = value;
            }    
        }

        void MoveCursor(int delta)
        {
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
            targetListPosition = listOffset + Vector2.up * selectedLevelIndex * levelScrollAmount;

            // show the description of the newly hovered level
            RefreshDescription();
        }

        void MoveTabCursor(int delta) {
            // don't move cursor if movement will send cursor outside the list
            if (selectedTabIndex+delta < 0 || selectedTabIndex+delta >= tabs.Length) return;

            levelTabTransform.GetChild(selectedTabIndex).gameObject.SetActive(false);
            selectedTabIndex += delta;
            levelTabTransform.GetChild(selectedTabIndex).gameObject.SetActive(true);

            RefreshTab();
            RefreshListedLevelText(selectedLevel, selectedText, true);
            RefreshCursor();
        }

        void RefreshDescription()
        {
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
            string newTabText = "";
            for (int i = 0; i < tabs.Length; i++)
            {
                // most readable code in ohio
                newTabText += (selectedTabIndex == i) ? "<color=white>" : "<color=#10FF10>";
                newTabText += (selectedTabIndex == i && selectedTabIndex > 0) ? "< " : "  ";

                SoloMenuTab currentTab = tabs[i];
                newTabText += currentTab.tabName;

                newTabText += (selectedTabIndex == i && selectedTabIndex < tabs.Length-1) ? " ></color>" : "  </color>";
            }

            tabText.text = newTabText;

            // update target tab scroll pos
            float newTabPos = 0;
            for (int i = 0; i < selectedTabIndex; i++)
            {
                newTabPos += (tabs[i].tabName.Length+tabWhitespacing) * tabScrollAmount;
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
    public class InputScriptEditor : Editor {
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