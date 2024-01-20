using System;
using Battle.Board;
using Battle.Cycle;
using Mirror;
using UnityEngine;
using VersusMode;

public class NetPlayer : NetworkBehaviour {
    public CharSelector charSelector;
    
    public GameBoard board;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        RegisterMessages();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ConnectToCharSelector();
    }

    bool connectedToCharSelector = false;
    public void ConnectToCharSelector() {
        if (connectedToCharSelector) return;

        if (isOwned) {
            charSelector = CharSelectMenu.Instance.p1Selector;
            SoloCharSelectController.instance.netPlayer = this;
        } else {
            charSelector = CharSelectMenu.Instance.p2Selector;
        }
        charSelector.Connect();
        connectedToCharSelector = true;
        
        OnSelectionIndex(0, selectionIndex);
        OnLockedIn(false, lockedIn);
    }

    private void OnDisable() {
        if (charSelector) charSelector.Disconnect();
    }

    public void RegisterMessages() {
        // NetworkClient.RegisterHandler<CharSelectionDataMessage>(OnCharSelectionData);
    }

    /// <summary>
    /// Data to send to the other client when they join.
    /// Describes the current character and lock status of the other client.
    /// </summary>
    public struct CharSelectionDataMessage : NetworkMessage {
        // index of selected character icon
        public int index;
        // whether or not this player is locked in already or not
        public bool lockedIn;
    }


    [SyncVar(hook = nameof(OnSelectionIndex))]
    public int selectionIndex = 0;

    public void OnSelectionIndex(int oldIndex, int newIndex) {
        if (!charSelector) ConnectToCharSelector();
        if (!charSelector) {
            Debug.LogWarning("Charselector not found!");
            return;
        }
        charSelector.SetSelection(newIndex);
        Debug.Log(charSelector + " new selection index: "+newIndex);
    }

    [SyncVar]
    public bool randomSelected;

    [SyncVar(hook = nameof(OnLockedIn))]
    public bool lockedIn;


    public void OnLockedIn(bool oldLockedIn, bool newLockedIn) {
        if (!charSelector) ConnectToCharSelector();
        if (!charSelector) {
            Debug.LogWarning("Charselector not found!");
            return;
        }
        if (randomSelected) {
            charSelector.randomBattler = CharSelectMenu.Instance.characterIcons[selectionIndex].battler;
        } else {
            charSelector.SetSelection(selectionIndex);
        }
        if (newLockedIn != charSelector.lockedIn) charSelector.ToggleLock();
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