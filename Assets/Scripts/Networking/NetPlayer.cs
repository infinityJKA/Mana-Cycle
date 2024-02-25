using System;
using Battle.Board;
using Battle.Cycle;
using Mirror;
using PostGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using VersusMode;

public class NetPlayer : NetworkBehaviour {
    public CharSelector charSelector;
    
    public GameBoard board;

    /// <summary>
    /// If this client has requested a rematch in the postgame menu.
    /// </summary>
    public bool rematchRequested = false;


    private NetPlayer enemyPlayer {get{return board.enemyBoard.netPlayer;}}

    private void Start() {
        DontDestroyOnLoad(gameObject);
        ConnectToCharSelector();
    }

    bool connectedToCharSelector = false;
    private void ConnectToCharSelector() {
        if (connectedToCharSelector) return;
        connectedToCharSelector = true;

        if (isOwned) {
            charSelector = CharSelectMenu.Instance.p1Selector;
            SoloCharSelectController.instance.netPlayer = this;
        } else {
            charSelector = CharSelectMenu.Instance.p2Selector;
        }
        charSelector.Connect();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
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
    public void CmdSetLockedIn() {
        RpcSetLockedIn(charSelector.selectedIcon.index, charSelector.isRandomSelected, charSelector.lockedIn);
    }


    [Command]
    public void CmdSetLockedIn(int index, bool randomSelected, bool lockedIn) {
        if (!connectedToCharSelector) ConnectToCharSelector();
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
        if (!connectedToCharSelector) ConnectToCharSelector();
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

        ManaCycle.instance.CreateCycle();

        Debug.LogWarning("sending init data: "+initData);
        RpcSynchronize(initData);

        // ready to show cycle to player now that cycle is initialized
        TransitionScript.instance.ReadyToFadeOut();
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSynchronize(BattleInitData initData) {
        Debug.LogWarning("received init data: "+initData);

        // if current scene is Mana Cycle, initialize using received data
        if (SceneManager.GetActiveScene().name == "ManaCycle") {
            InitializeBattle(initData);
        } 
        // otherwise, run initialization once the scene is loaded by setting up a listener for sceneLoaded
        else {
            Debug.Log("waiting for scene load before battle initialize");
            latestInitData = initData;
            waitingForSceneStartBeforeInit = true;
        }
    }

    private BattleInitData latestInitData;
    public bool waitingForSceneStartBeforeInit = false;

    /// <summary>
    /// In the case init data was received before the baattle scene loads, this will init using the data received earlier in ManaCycle.cs's Start method
    /// </summary>
    public void OnBattleSceneLoaded() {
        if (!waitingForSceneStartBeforeInit) return;
        waitingForSceneStartBeforeInit = false;

        Debug.Log("delayed initialization");
        InitializeBattle(latestInitData);
    }

    /// <summary>
    /// Initializes the data on the CLIENT using data from the HOST (rng data).
    /// </summary>
    /// <param name="initData">contains the individual player rng seeds and the randomized cycle colors</param>/
    public void InitializeBattle(BattleInitData initData) {
        ManaCycle.instance.Boards[0].rngManager.SetSeed(initData.nonHostSeed);
        ManaCycle.instance.Boards[1].rngManager.SetSeed(initData.hostSeed);
        ManaCycle.SetCycle(initData.cycle);
        
        // Creates the cycle objects and initializes various things on each board.
        ManaCycle.instance.CreateCycle();

        // match will officially start 4 seconds after this initialize battle command is executed on the client
        double startTime = NetworkTime.time + 4;
        ManaCycle.instance.CountdownHandler.StartTimerNetworkTime(startTime);

        // let host know this client is ready and let it know the time that the match will start
        Debug.Log("Sending ready cmd to host - start time: "+startTime+", time: "+NetworkTime.time+", timeUntilStart: 4?");
        enemyPlayer.CmdClientReady(startTime);

        // ready to show cycle to player now that cycle is initialized
        TransitionScript.instance.ReadyToFadeOut();
    }

    [Command]
    /// <summary>
    /// Message sent from the client (player 2) to the host when they are ready for the match to begin
    /// </summary>
    /// <param name="startTime">Synchronized time client has decided the game will start</param>
    private void CmdClientReady(double startTime) {
        double timeUntilStart = startTime - NetworkTime.time;
        Debug.Log("Client is ready - start time: "+startTime+", time: "+NetworkTime.time+", time until start: "+timeUntilStart);

        ManaCycle.instance.CountdownHandler.StartTimerNetworkTime(startTime);
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


    /// <summary>
    /// Clear the current color in the chain/cascade and send damage to opponent (as instant damage for now as of writing)
    /// </summary>
    /// <param name="startup">is this is true, will just show spellcast startup effect and nothing esle</param>
    /// <param name="damageSent">amount of damage sent from this clear's damage, not including countering own damage or making own shields. 
    /// (includes damaging opponent's shield.)</param>
    [Command]
    public void CmdAdvanceChain(bool startup, int damageSent) {
        RpcAdvanceChain(startup, damageSent);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcAdvanceChain(bool startup, int damageSent) {
        if (startup) {
            board.StartupEffect();
        } else {
            board.AdvanceChainAndCascade();
            board.enemyBoard.EvaluateInstantIncomingDamage(damageSent);
        }
    }


    [Command]
    public void CmdUseAbility(int[] data) {
        RpcUseAbility(data);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcUseAbility(int[] data) {
        board.abilityManager.abilityData = data;
        board.abilityManager.UseAbility();
    }

    [Command]
    public void CmdUpdateDamageQueue(int shield, int[] damageQueue) {
        RpcUpdateDamageQueue(shield, damageQueue);
    }

    /// <summary>
    /// Shows to the other client the current state of this player's damage queue.
    /// </summary>
    /// <param name="damageQueueIndex"></param>
    /// <param name="justQueued">If damage was just added to the queue. SFX will be played</param> 
    [ClientRpc(includeOwner = false)]
    private void RpcUpdateDamageQueue(int shield, int[] damageQueue) {
        for (int i=0; i<6; i++) {
            board.hpBar.DamageQueue[i].SetDamage(damageQueue[i]);
        }
        board.PlaySFX("dmgShoot", pitch: 1f + board.hpBar.DamageQueue[0].dmg/1000f, volumeScale: 1.3f);
    }

    // Initiate a game rematch.
    // Called by client when they request a rematch.
    // Also called by the host when they themselves request a rematch (though it isn't sent as a packet.)
    [Command]
    public void CmdRematch() {
        // Initialize a rematch if the host has also requested a rematch.
        if (rematchRequested && enemyPlayer.rematchRequested) {
            RpcStartRematch();
        }
    }

    // Called by the host when a rematch is starting.
    // Reloads the ManaCycle scene on both host and client when CmdRematch calls from host.
    [ClientRpc]
    private void RpcStartRematch() {
        board.winMenu.Replay();
    }
}