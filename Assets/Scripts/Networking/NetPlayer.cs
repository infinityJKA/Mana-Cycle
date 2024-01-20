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
            SoloCharSelectController.instance.netPlayer = this;
        } else {
            charSelector = CharSelectMenu.Instance.p2Selector;
        }
        charSelector.Connect();
    }

    private void OnDisable() {
        if (charSelector) charSelector.Disconnect();
    }

    [Command]
    public void CmdSetSelectedBattlerIndex(int index) {
        charSelector.SetSelection(index);
    }

    /// <summary>
    /// Send to a client the current selected battler's index of this player.
    /// Wiil not send to the player that called this; it only mirrors the selection to the other player
    /// </summary>
    /// <param name="index">the index of the character icon in the scene's CharSelectMenu</param>
    [ClientRpc(includeOwner = false)]
    public void RpcSetSelectedBattlerIndex(int index) {
        charSelector.SetSelection(index);
    }

    [Command]
    public void CmdSetLockedIn(int index, bool randomSelected, bool lockedIn) {
        RpcSetLockedIn(index, randomSelected, lockedIn);
    }

    /// <summary>
    /// Mirrors the locked-in status to the other player.
    /// </summary>
    /// <param name="index">index of battler selected - this may also be random battler that the opponent's client landed on</param>
    /// <param name="randomSelected">whether or not opponent selected the random battler</param>
    /// <param name="lockedIn">lock-in status, true if player locked in, false if player un-locked in</param>
    [ClientRpc(includeOwner = false)]
    public void RpcSetLockedIn(int index, bool randomSelected, bool lockedIn) {
        if (randomSelected) {
            charSelector.randomBattler = CharSelectMenu.Instance.characterIcons[index].battler;
        } else {
            charSelector.SetSelection(index);
        }
        if (lockedIn != charSelector.lockedIn) charSelector.ToggleLock();
    }
}