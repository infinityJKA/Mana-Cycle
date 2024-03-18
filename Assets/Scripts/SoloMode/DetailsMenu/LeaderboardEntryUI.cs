using LootLocker.Requests;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour {
    public TMP_Text placeLabel, usernameLabel, scoreLabel;
    public Image avatarImage;

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void ShowMember(LootLockerLeaderboardMember member) {
        gameObject.SetActive(true);
        placeLabel.text = ""+member.rank;
        usernameLabel.text = member.member_id;
        scoreLabel.text = ""+member.score;
    }
}