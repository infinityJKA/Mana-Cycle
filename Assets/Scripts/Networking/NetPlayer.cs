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

        ManaCycle.instance.CreateCycleObjects();

        Debug.Log("sending init data: "+initData);
        RpcSynchronize(initData);

        // ready to show cycle to player now that cycle is initialized
        TransitionScript.instance.ReadyToFadeOut();
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSynchronize(BattleInitData initData) {
        Debug.Log("received init data: "+initData);

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
        ManaCycle.instance.CreateCycleObjects();

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
        // if a piece deson't exist, probably means there is a recovering/not recovering desync.
        // not a big deal, because PlacePiece() actually decides where the final position of the piece is.
        // so if there isn't a piece here just ignore the RPC
        if (!board.GetPiece()) return;

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

    /// <summary>
    /// Sends lots of info to the other client, including the piece position and orientation,
    /// hp after the damage cycle calculation in case of desync,
    /// and pieceId and dropIndex to detect desyncs.
    /// </summary>
    /// <param name="targetColumn"></param>
    /// <param name="rotation"></param>
    /// <param name="pieceId"></param>
    /// <param name="dropIndex"></param>
    /// <param name="hp"></param>
    [Command]
    public void CmdPlacePiece(int targetColumn, Piece.Orientation rotation, int hp, int pieceId, int dropIndex) {
        RpcPlacePiece(targetColumn, rotation, hp, pieceId, dropIndex);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcPlacePiece(int targetColumn, Piece.Orientation rotation, int hp, int pieceId, int dropIndex) {
        // possible improvement: If piece id is desynced, ask the other client to send their current board state and piece index
        // may be needed for UDP which may be switched to

        //ensure the piece being dropped matches the id of the piece the client is dropping, same with drop index
        Debug.Assert(board.GetPiece().id == pieceId);
        Debug.Assert(board.pieceDropIndex+1 == dropIndex);

        // match rotation and column that the other client says before dropping
        board.GetPiece().SetRotation(rotation);
        board.SetPiecePosition(targetColumn, board.GetPiece().GetRow());

        // ensure piece is not placed inside the ground
        // since column and rotation are ensured to match,
        // gravity will ensure final tiles' positions match the tiles of the client owning this board.
        while (!board.ValidPlacement()) {
            board.MovePiece(0, -1);
        }

        // drop index assertion will happen within this method, to make sure drop order is not jumbled
        board.PlacePiece();

        board.SetHp(hp);
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
        // NOTE: this might not be needed but want to ensure proper sync, remove if bandwidth concerns
        CmdUpdateDamageQueue(); 
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

    // TODO: BIG TODO
    // fix the synchronization of aqua/zman tiles.
    // the client receiving the trash tiles needs to be the one to authoritate where they land,
    // not the client sending the trash tiles.
    [ClientRpc(includeOwner = false)]
    private void RpcUseAbility(int[] data) {
        board.abilityManager.abilityData = data;
        board.abilityManager.UseAbility();
    }

    /// <summary>
    /// Call from a client when damage is queued/countered from the other player.
    /// </summary>
    [Command]
    public void CmdUpdateDamageQueue() {
        int[] damageQueue = new int[6];
        for (int i=0; i<6; i++) {
            damageQueue[i] = board.hpBar.DamageQueue[i].dmg;
        }
        RpcUpdateDamageQueue(board.shield, damageQueue);
    }

    /// <summary>
    /// Shows to the other client the current state of this player's damage queue.
    /// Mirror HP, shield and damagequeue in case they have been desynced.
    /// </summary>
    [ClientRpc(includeOwner = false)]
    private void RpcUpdateDamageQueue(int shield, int[] damageQueue) {
        board.SetShield(shield);
        for (int i=0; i<6; i++) {
            board.hpBar.DamageQueue[i].SetDamage(damageQueue[i]);
        }
        board.PlayDamageShootSFX();
    }

    /// <summary>
    /// Call this from a client after it is damaged to mirror hp accurately.
    /// THis is really only used by the trash damage timer as placePiece also sends hp.
    /// </summary>
    /// <param name="hp">amount of hp to set to</param>
    /// <param name="intensity">if above 0, a damage animation will play with the given intensity.</param>
    public void CmdUpdateHp(int hp, float intensity) {
        RpcUpdateHp(hp, intensity);
    }

    /// <summary>
    /// Update current HP on this client.
    /// Allowed to kill the player because the other client has verified themself they have this HP
    /// </summary>
    /// <param name="hp">amount of hp to set to</param>
    /// /// <param name="intensity">if above 0, a damage animation will play with the given intensity.</param>
    [ClientRpc(includeOwner = false)]
    private void RpcUpdateHp(int hp, float intensity) {
        board.SetHp(hp, allowDeath: true);
        if (intensity > 0) board.DamageShake(intensity);
    }

    // Initiate a game rematch.
    // Called by client when they request a rematch.
    // Also called by the host when they themselves request a rematch (though it isn't sent as a packet.)
    [Command]
    public void CmdRematch() {
        RpcStartRematch();
    }

    // Called by the host when a rematch is starting.
    // Reloads the ManaCycle scene on both host and client when CmdRematch calls from host.
    [ClientRpc]
    private void RpcStartRematch() {
        Debug.Log(board+" is requesting a rematch");
        // Initialize a rematch if the host has also requested a rematch.
        if (rematchRequested && enemyPlayer.rematchRequested) {
            Debug.Log("both players request rematch; Rematch starting");
            board.winMenu.Replay();
        }
    }
}