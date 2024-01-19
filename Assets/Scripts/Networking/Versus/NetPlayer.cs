using Mirror;
using UnityEngine;
using VersusMode;

public class NetPlayer : NetworkBehaviour {
    [SerializeField] private CharSelector charSelector;

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