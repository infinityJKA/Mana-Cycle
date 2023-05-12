using UnityEngine;
using System;

using ConvoSystem;

using Sound;

namespace SoloMode {
    public class LevelLister : MonoBehaviour
    {
        // morgan please comment your code in the future :(

        /** Conversation handler, to run conversation when level is selected */
        [SerializeField] private ConvoHandler convoHandler;

        /** List of levels to select */
        private Level[] levelsList;

        // List level object caches
        /** list text component where the lines are written out */
        [SerializeField] private TMPro.TextMeshProUGUI listText;
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
        /** amount to scroll by for each line. set to font size on start */
        private float levelScrollAmount;
        public float tabScrollAmount;

        /** Index of current level selected */
        private int[] selectedLevelIndex;
        private int selectedTabIndex;
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
        

        // currently unused references
        [SerializeField] GameObject upArrow;
        [SerializeField] GameObject downArrow;

        [SerializeField] SoloMenuTab[] tabs;


        private static int tabWhitespacing = 4;

        // Start is called before the first frame update
        void Start()
        {
            selectedLevelIndex = new int[tabs.Length];
            selectedTabIndex = 0;
            levelsList = tabs[selectedTabIndex].levelsList;
            focused = true;
            // decLine = (listText.font.faceInfo.descentLine);

            // last level and tab selected when in this window.
            if (Storage.lastTabSelectedIndex == -1) Storage.lastTabSelectedIndex = 0;
            else selectedTabIndex = Storage.lastTabSelectedIndex;

            if (Storage.lastLevelSelectedIndex == -1) Storage.lastLevelSelectedIndex = GetNextLevel();
            else selectedLevelIndex[selectedTabIndex] = Storage.lastLevelSelectedIndex;

            Storage.lastLevelSelectedIndex = selectedLevelIndex[selectedTabIndex];

            listOffset = listText.rectTransform.anchoredPosition;
            tabOffset = tabText.rectTransform.offsetMin;
            levelScrollAmount = listText.fontSize;

            // idk how to get char width. public field guess&check for now
            // tab scroll amount is number of px per character
            // tabScrollAmount = tabText.textInfo.characterInfo[1].xAdvance; 

            ClampSelections();
            RefreshList();
        }

        // Update is called once per frame
        void Update()
        {
            if (!focused) return;

            foreach (InputScript inputScript in inputScripts) {

                if (Input.GetKeyDown(inputScript.Up))
                {
                    selectedLevelIndex[selectedTabIndex]--;
                    ClampSelections();
                    StoreSelections();
                    RefreshList();
                    SoundManager.Instance.PlaySound(moveSFX, pitch : 1.18f);
                }

                if (Input.GetKeyDown(inputScript.Down))
                {
                    selectedLevelIndex[selectedTabIndex]++;
                    ClampSelections();
                    StoreSelections();
                    RefreshList();
                    SoundManager.Instance.PlaySound(moveSFX, pitch : 1.06f);
                }

                if (Input.GetKeyDown(inputScript.Right))
                {
                    selectedTabIndex++;
                    ClampSelections();
                    StoreSelections();
                    RefreshList();
                    SoundManager.Instance.PlaySound(swapTabSFX, pitch : 1.56f);
                }

                if (Input.GetKeyDown(inputScript.Left))
                {
                    selectedTabIndex--;
                    ClampSelections();
                    StoreSelections();
                    RefreshList();
                    SoundManager.Instance.PlaySound(swapTabSFX, pitch : 1.68f);
                }

                // pause - go back to main menu
                if (Input.GetKeyDown(inputScript.Pause))
                {
                    GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene("MainMenu", reverse : true);
                }

                // cast - open selected level
                if (Input.GetKeyDown(inputScript.Cast))
                {
                    if (!levelsList[selectedLevelIndex[selectedTabIndex]].RequirementsMet()) 
                    {
                        SoundManager.Instance.PlaySound(errorSFX);
                        return;
                    }

                    StoreSelections();
                    Storage.level = levelsList[selectedLevelIndex[selectedTabIndex]]; 
                    Storage.gamemode = Storage.GameMode.Solo;
                    convoHandler.StartLevel(levelsList[selectedLevelIndex[selectedTabIndex]]);
                    focused = false;
                    Storage.levelSelectedThisInput = true;
                    SoundManager.Instance.PlaySound(selectSFX);
                }
            }

            // smoothly update displayed y position of level list
            listText.rectTransform.anchoredPosition = 
            Vector2.SmoothDamp(listText.rectTransform.anchoredPosition, targetListPosition, ref vel, 0.1f);

            tabText.rectTransform.offsetMin = 
            Vector2.SmoothDamp(tabText.rectTransform.offsetMin, targetTabPosition, ref vel2, 0.1f);
        }

        private void StoreSelections()
        {
            Storage.lastTabSelectedIndex = selectedTabIndex;
            Storage.lastLevelSelectedIndex = selectedLevelIndex[selectedTabIndex];
        }

        private void ClampSelections()
        {
            selectedTabIndex = Math.Clamp(selectedTabIndex, 0, tabs.Length-1);
            levelsList = tabs[selectedTabIndex].levelsList;
            selectedLevelIndex[selectedTabIndex] = Math.Clamp(selectedLevelIndex[selectedTabIndex], 0, levelsList.Length-1);
        }


        private static int flavorLineCount = 30;
        void RefreshList()
        {
            string newText = "";
            // add and subtract for extra lines at the start and end of list
            for (int i = -flavorLineCount; i < levelsList.Length + flavorLineCount; i++)
            {
                // flavor lines
                if (i < 0 || i >= levelsList.Length){
                    newText += "################" + "\n";
                }
                else{
                    Level level = levelsList[i];

                    bool cleared = level.GetCleared();

                    if (i == selectedLevelIndex[selectedTabIndex] && level.RequirementsMet()) newText += " <color=#FFFFFF>";

                    else if (!level.RequirementsMet()) newText += "<color=#015706>";

                    else newText += "<color=#00ff10>";

                    newText += level.levelName;

                    if (i == selectedLevelIndex[selectedTabIndex]) newText += " <";
                    newText += " </color>";

                    // clear check
                    if (cleared)
                    {
                        newText += "<color=#00ffdf> X</color>";
                    }
                    else
                    {
                        newText += "<color=#000000> X</color>";
                    }

                    newText += "\n";
                }

            }
            listText.text = newText;

            // display the description and time of the selected level
            Level selectedLevel = levelsList[selectedLevelIndex[selectedTabIndex]];
            descriptionText.text = selectedLevel.description;

            bool selectedCleared = selectedLevel.GetCleared();
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
            

            // update the targeted level scroll position
            targetListPosition = listOffset + Vector2.up * (flavorLineCount+selectedLevelIndex[selectedTabIndex]) * levelScrollAmount;

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
            for (int i = 0; i < levelsList.Length; i++)
            {
                if (!(PlayerPrefs.GetInt(levelsList[i].levelName+"_Cleared", 0) == 1)) return i;
            }
            // if all levels are cleared, start at 0
            return 0;
        }
    }
}