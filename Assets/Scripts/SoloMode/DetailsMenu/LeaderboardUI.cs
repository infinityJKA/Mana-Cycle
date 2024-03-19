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

    // remove data when changing scenes
    private void OnDestroy() {
        Debug.Log("loaded leaderboard data cleared");
        LeaderboardManager.RemoveAllLoadedEntries();
    }

    public void HideEntries() {
        for (int i = 0; i < entryUIList.Length; i++) {
            entryUIList[i].Hide();
        }
    }

    public void LoadCurrentPage(bool refreshing = false) {
        var entryList = LeaderboardManager.GetEntryList(levelLister.selectedLevel, leaderboardType);
        // if page is already loaded, display and don't fetch it again
        if (entryList.HasPage(currentPage)) {
            DisplayCurrentPage();
            return;
        }
        // don't fetch if already currently loading
        if (entryList.loadingPage == currentPage) return;

        // TODO: use loading spinner instead of hiding entries when refreshing as opposed to loading for first time.
        HideEntries();
        loadingLabel.gameObject.SetActive(true);
        loadingLabel.colorGradientPreset = greyGradient;
        if (refreshing) {
            loadingLabel.text = "Refreshing...";
        } else {
            loadingLabel.text = "Loading...";
        }

        int pageWhenRequestSent = currentPage;
        Debug.Log("Loading data for level "+levelLister.selectedLevel+" mode:"+leaderboardType+" page:"+currentPage);
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
    public void DisplayCurrentPage() {
        if (!LeaderboardManager.IsDataLoaded(levelLister.selectedLevel, leaderboardType, currentPage)) {
            HideEntries();
            return;
        };

        LootLockerLeaderboardMember[] entries = LeaderboardManager.RetrieveLoadedData(levelLister.selectedLevel, leaderboardType, currentPage);

        if (entries == null) {
            HideEntries();
            loadingLabel.gameObject.SetActive(true);
            loadingLabel.text = "No scores yet...";
        } else {
            loadingLabel.gameObject.SetActive(false);
            int i;
            for (i = 0; i < entries.Length; i++) {
                entryUIList[i].ShowMember(entries[i]);
            }
            for (; i < 10; i++) {
                entryUIList[i].Hide();
            }
        }
    }

    public void RefreshCurrentPage() {
        var entryList = LeaderboardManager.GetEntryList(levelLister.selectedLevel, leaderboardType);
        entryList.pages.Remove(currentPage);
    }
}

