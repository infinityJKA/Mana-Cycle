using System;
using Battle.Board;
using Battle.Cycle;
using Mirror;
using UnityEngine;
using VersusMode;

public class NetPlayer : NetworkBehaviour {
    private CharSelector charSelector;
    
    public GameBoard board;

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

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
        RpcSetSelectedBattlerIndex(index);
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


    [Command]
    public void CmdStartGame() {
        RpcStartGame();
    }

    [ClientRpc(includeOwner = false)]
    public void RpcStartGame() {
        charSelector.menu.StartIfReady();
    }


    public readonly static System.Random seedGenerator = new System.Random();

    // Called when the battle begins. Only runs on the host (player 1).
    // Sets up RNG and other things to synchronize with opponent
    
    public void CmdBattleInit() {
        ManaCycle.GenerateCycle();
        BattleInitData initData = new BattleInitData
        {
            // player 1 generates the RNG seeds for both players.
            hostSeed = seedGenerator.Next(),
            nonHostSeed = seedGenerator.Next(),
            cycle = ManaCycle.cycle,
        };

        ManaCycle.instance.Boards[0].rngManager.SetSeed(initData.hostSeed);
        ManaCycle.instance.Boards[1].rngManager.SetSeed(initData.nonHostSeed);

        Debug.LogWarning("sending init data: "+initData);
        RpcSynchronize(initData);
    }


    [System.Serializable]
    public struct BattleInitData {
        public int hostSeed;
        public int nonHostSeed;
        public ManaColor[] cycle;

        public override string ToString()
        {
            return $"host seed: {hostSeed}, nonhost seed: {nonHostSeed}, cycle: {string.Join(',', cycle)}";
        }
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSynchronize(BattleInitData initData) {
        Debug.LogWarning("received init data: "+initData);
        ManaCycle.instance.Boards[0].rngManager.SetSeed(initData.nonHostSeed);
        ManaCycle.instance.Boards[1].rngManager.SetSeed(initData.hostSeed);
        ManaCycle.SetCycle(initData.cycle);
    }


    [Command]
    public void CmdMovePiece(int targetColumn, Piece.Orientation rotation) {
        RpcMovePiece(targetColumn, rotation);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcMovePiece(int targetColumn, Piece.Orientation rotation) {
        // ensure piece is not placed inside the ground
        board.GetPiece().SetRotation(rotation);
        board.SetPiecePosition(targetColumn, board.GetPiece().GetRow());
        while (!board.ValidPlacement()) {
            board.MovePiece(0, -1);
        }
    }

    [Command]
    public void CmdSetQuickfall(bool quickfalling) {
        RpcSetQuickfall(quickfalling);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSetQuickfall(bool quickfalling) {
        board.quickFall = quickfalling;
    }


    [Command]
    public void CmdPlacePiece(int targetColumn, Piece.Orientation rotation) {
        RpcPlacePiece(targetColumn, rotation);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcPlacePiece(int targetColumn, Piece.Orientation rotation) {
        // possible improvement: If piece index is desynced, ask the other client to send their current board state and piece index
        // may be needed for UDP which may be switched to

        // ensure piece is not placed inside the ground
        board.GetPiece().SetRotation(rotation);
        board.SetPiecePosition(targetColumn, board.GetPiece().GetRow());
        while (!board.ValidPlacement()) {
            board.MovePiece(0, -1);
        }

        board.PlacePiece();
    }


    [Command]
    public void CmdAdvanceChain(bool startup) {
        RpcAdvanceChain(startup);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcAdvanceChain(bool startup) {
        if (startup) {
            board.StartupEffect();
        } else {
            board.AdvanceChainAndCascade();
        }
    }
}