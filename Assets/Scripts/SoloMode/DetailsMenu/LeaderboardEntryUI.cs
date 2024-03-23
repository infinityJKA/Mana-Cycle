using LootLocker.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour {
    public TMP_Text placeLabel, usernameLabel, scoreLabel;
    public Image avatarImage;

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void ShowMember(LootLockerLeaderboardMember member) {
        gameObject.SetActive(true);
        placeLabel.text = ""+member.rank;
        string name = member.player.name;
        if (name == null || name == "") name = "Guest "+member.member_id;
        usernameLabel.text = name;
        scoreLabel.text = ""+member.score;
    }
}