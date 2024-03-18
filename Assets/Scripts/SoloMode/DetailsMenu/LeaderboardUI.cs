using System.Collections.Generic;
using LootLocker.Requests;
using SoloMode;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour {
    [SerializeField] private LevelLister levelLister;


    [SerializeField] private TMP_Text loadingLabel;

    [SerializeField] private TMP_ColorGradient greyGradient, redGradient;

    // should select with the mode select at top of leaderboard window
    [SerializeField] private LeaderboardType leaderboardType;

    // pages are in counts of 10 entries per page. (page starts at 0)
    int currentPage;

    [SerializeField] private Transform entryUiListTransform;
    private LeaderboardEntryUI[] entryUIList;

    private void Start() {
        entryUIList = entryUiListTransform.GetComponentsInChildren<LeaderboardEntryUI>();
        HideEntries();
    }

    public void HideEntries() {
        for (int i = 0; i < entryUIList.Length; i++) {
            entryUIList[i].Hide();
        }
    }

    public void LoadCurrentPage() {
        // display if already loaded
        if (LeaderboardManager.IsDataLoaded(levelLister.selectedLevel, leaderboardType, currentPage)) {
            DisplayCurrentPage();
            return;
        }

        HideEntries();
        loadingLabel.gameObject.SetActive(true);
        loadingLabel.colorGradientPreset = greyGradient;
        loadingLabel.text = "Loading...";

        int pageWhenRequestSent = currentPage;
        LeaderboardManager.LoadLeaderboardData(levelLister.selectedLevel, leaderboardType, currentPage, (callback) => {
            if (!callback.success) {
                loadingLabel.colorGradientPreset = redGradient;
                loadingLabel.text = "Error retreiving scores";
                return;
            }

            if (
                callback.level == levelLister.selectedLevel
                && callback.type == leaderboardType
                && callback.page == currentPage
            ) {
                DisplayCurrentPage();
            };
        });
    }

    // Display current page.
    // If not, request to load the data and then this will be called again.
    public void DisplayCurrentPage() {
        LootLockerLeaderboardMember[] entries = LeaderboardManager.RetrieveLoadedData(levelLister.selectedLevel, leaderboardType, currentPage);
        
        loadingLabel.gameObject.SetActive(false);
        for (int i = 0; i < entries.Length; i++) {
            entryUIList[i].ShowMember(entries[i]);
        }
        
    }
}

