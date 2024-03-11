using System;
using Battle.Board;
using Battle.Cycle;
using Mirror;
using PostGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using VersusMode;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class NetPlayer : NetworkBehaviour {
    public CharSelector charSelector;
    
    public GameBoard board;

    /// <summary>
    /// the username of this player. may either be retreived from steam or some other relay type service, may change in the future
    /// </summary>
    public string username {get; private set;} = "Connecting...";


    private NetPlayer enemyPlayer {get{return board.enemyBoard.netPlayer;}}

    private void Start() {
        DontDestroyOnLoad(gameObject);

        // if using steam & this is local player, set username to be steam local name
        #if !DISABLESTEAMWORKS
            if (NetManager.IsUseSteam()) {
                SetUsername(SteamFriends.GetPersonaName());
            }
        #endif

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
        
        charSelector.Connect(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void OnDisable() {
        if (charSelector) charSelector.Disconnect();
    }

    public void SetUsername(string username) {
        this.username = username;
        
        // in charselect, update username immediately when received
        if (charSelector != null) {
            charSelector.SetUsername(username);
        }
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

        // if current scene is Mana Cycle & cycle has finished initializing, initialize using received data
        if (SceneManager.GetActiveScene().name == "ManaCycle" && ManaCycle.initializeFinished) {
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

        // match will officially start 5 seconds after this initialize battle command is executed on the client
        double startTime = NetworkTime.time + 5;
        ManaCycle.instance.CountdownHandler.StartTimerNetworkTime(startTime);

        // let host know this client is ready and let it know the time that the match will start
        Debug.Log("Sending ready cmd to host - start time: "+startTime+", time: "+NetworkTime.time+", timeUntilStart: 5?");
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

    // Data to reflect the FULL state of this board to the other client.
    [System.Serializable]
    public struct FullBoardData {
        // 8x20 board
        // tile-byte format:
        // 0    1   2   3   4   5   6       7
        // none red yel gre blu pur goldmine zman
        // 8    9   10  11  12  13  14      15
        // 8-15 same as aobve but trash tile (only applies to red thru purple)
        public byte[] tiles;  // byte[160]

        // current piece [1] + preview [3] * 3 colors each * 1 byte per color = 12
        public byte[] pieces; // byte[12]

        // ability piece preview overrides. may override what's on the piece preview
        // 0        1       2       3
        // none     Sword   Bomb    Crystal
        public byte[] pieceOverrides; // byte[4]

        // index of piece rng in the seeded order
        // (does not include the initial generation for the ability rng seed)
        public int pieceRngIndex;

        // index of ability rng in the seeded order
        public int abilityRngIndex;
    }

    [Command]
    public void CmdSendFullBoardData() {
        FullBoardData data = new FullBoardData();

        byte[] tiles = new byte[160];
        for (int c = 0; c < GameBoard.width; c++) {
            for (int r = 0; r < GameBoard.height; r++) {
                int i = c*GameBoard.width + r;
                Tile tile = board.tiles[r,c];
                tiles[i] = TileToColorByte(tile);
            }
        }
        data.tiles = tiles;

        byte[] pieces = new byte[12];
        pieces[0] = TileToColorByte(board.GetPiece().GetCenter());
        pieces[1] = TileToColorByte(board.GetPiece().GetTop());
        pieces[2] = TileToColorByte(board.GetPiece().GetRight());

        Piece nextPiece = board.piecePreview.GetNextPiece();
        pieces[3] = TileToColorByte(nextPiece.GetCenter());
        pieces[4] = TileToColorByte(nextPiece.GetTop());
        pieces[5] = TileToColorByte(nextPiece.GetRight());

        for (int i = 0; i < PiecePreview.previewLength; i++) {
            Piece previewPiece = board.piecePreview.GetPreviewPiece(i);
            pieces[(i+2)*3] = TileToColorByte(previewPiece.GetCenter());
            pieces[(i+2)*3 + 1] = TileToColorByte(previewPiece.GetTop());
            pieces[(i+2)*3 + 2] = TileToColorByte(previewPiece.GetRight());
        }
        data.pieces = pieces;

        byte[] pieceOverrides = new byte[4];
    }

    private byte TileToColorByte(Tile tile) {
        if (tile == null) {
            return 0;
        } else {
            return (byte)((byte)(tile.color) + 1);
        }
    }

    [ClientRpc(includeOwner = false)]
    private void RpcReceiveFullBoardData(FullBoardData data) {
        
    }

    public enum PostGameIntention {
        Undecided,
        Rematch,
        CharSelect
    }

    public PostGameIntention postGameIntention;

    // Set the intention of this player - None, Rematch, CharacterSelect.
    // Once both players have selected the same non-None intention then that button will be selected on both clients.
    [Command]
    public void CmdSetPostGameIntention(PostGameIntention intention)
    {
        RpcSetPostGameIntention(intention);
    }

    [ClientRpc]
    private void RpcSetPostGameIntention(PostGameIntention intention) {
        postGameIntention = intention;

        Debug.Log(gameObject+" intention: "+postGameIntention);

        if (postGameIntention == enemyPlayer.postGameIntention) {
            if (postGameIntention == PostGameIntention.Rematch) {
                PostGameMenu.Replay();
            }
            else if (postGameIntention == PostGameIntention.CharSelect) {
                PostGameMenu.BackToCSS();
            }
        }
    }
}