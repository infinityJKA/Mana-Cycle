using Mirror;
using UnityEngine;
using VersusMode;

public class NetPlayer : NetworkBehaviour {
    [SerializeField] private CharSelector charSelector;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isOwned) {
            charSelector = CharSelectMenu.Instance.p1Selector;
        } else {
            charSelector = CharSelectMenu.Instance.p2Selector;
        }
        charSelector.Connect();
    }

    private void OnDisable() {
        if (charSelector) charSelector.Disconnect();
    }

    [Command]
    private void CmdSetSelectedBattlerIndex(int index) {
        RpcSetSelectedBattlerIndex(index);
    }

    [ClientRpc]
    private void RpcSetSelectedBattlerIndex(int index) {
        charSelector.SetSelection(index);
    }

    public void HostGame() {
        string matchID = Matchmaker.GetRandomMatchId();
    }
}