using System;
using UnityEngine;
using UnityEngine.UI;

using Sound; 
using Animation;
using TMPro;

using Achievements;
using Mono.CSharp;
using UnityEngine.Localization.Settings;

namespace Battle.Board {
    /// <summary>
    /// Handles the mana bar and abilities for the GameBoard. Party/solo mode only
    /// </summary>
    public class AbilityManager : MonoBehaviour {
        // cached GameBoard
        private GameBoard board;

        // Image that has the material that controls the ability bar visuals
        [SerializeField] private Image abilityBarImage;

        /// <summary>Material that controls the ability bar shader visuals</summary>
        private Material abilityBarMaterial;
        
        // used for some abilities
        [SerializeField] public GameObject singlePiecePrefab;

        // Symbol list that appears near the cycle, used by Psychic's Foresight
        [SerializeField] public Transform symbolList;

        // Psychic's foresight icon prefab
        [SerializeField] public GameObject foresightIconPrefab;

        [SerializeField] private GameObject manaFillSFX, foresightActivateSFX, foresightConsumeSFX;

        /// <summary>Current amount of mana the player has generated</summary>
        public int mana {get; private set;}

        // used for synchronizing ability with the opponent's client.
        public int[] abilityData;

        // Timer for when to stop Electro's ability
        private float abilityEndTime;
        public bool thunderRushActive = false;
        [SerializeField] private Transform thunderIconPrefab;
        [SerializeField] private GameObject thunderActivateSFX,heroicShieldSFX;

        [SerializeField] private TMP_Text recoveryGaugeText;

        public int recoveryGaugeAmount {get; private set;}

        public StatusConditionManager scm;
        public float statusTime, statusDamageTime;
        public StatusConditions statusCondition;
        public int selectedStock;
        private string rejctedCard = "CREDIT CARD REJECTED\n\nINSUFFICIENT FUNDS BUDDY";
        private string rejctedCardjp = "あなたのクレジットカードは拒否されました。貧しくないときに戻ってきてください。";


        void Awake()
        {
            board = GetComponent<GameBoard>();
            enabled = Settings.current.enableAbilities;
            abilityBarMaterial = new Material(abilityBarImage.material);
            abilityBarImage.material = abilityBarMaterial;
            RefreshManaBar();
        }

        void Update(){
            if(thunderRushActive){
                Debug.Log("THUNDER RUSH ACTIVE "+ abilityEndTime+" !< "+Time.time);
                if(abilityEndTime <= Time.time){
                    Destroy(symbolList.GetChild(0).gameObject);
                    thunderRushActive = false;
                }
            }
            if(statusCondition != StatusConditions.NoCondition){
                // For taking damage from condition
                if(statusDamageTime + 1 - Time.time <= 0 && !board.singlePlayer){
                    if(statusCondition == StatusConditions.Poison ){
                        board.hpBar.DamageQueue[0].AddDamage(10);
                    }
                    else if(statusCondition == StatusConditions.PoisonSwapped){
                        board.hpBar.DamageQueue[0].AddDamage(50);
                    }
                    else if(statusCondition == StatusConditions.Fire && !board.recoveryMode){
                        board.SetHp(board.hp - 5);
                    }
                    else if(statusCondition == StatusConditions.FireSwapped && !board.recoveryMode){
                        board.SetHp(board.hp - 50);
                    }
                    else if(statusCondition == StatusConditions.Pure){
                        board.Heal(7);
                        board.hpBar.Refresh();
                    }
                    statusDamageTime = Time.time;
                }
                // For status effect timing
                if(statusTime + 10 - Time.time <= 0){
                    if(board.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.StatusCondition){
                        StatusConditions temp = statusCondition;
                        while(temp == statusCondition){
                            temp = scm.RandomStatusCondition();
                        }
                        statusCondition = temp;
                        statusTime = Time.time;
                    }
                    else{
                        statusCondition = StatusConditions.NoCondition;
                    }
                    scm.UpdateStatusIcon(statusCondition);
                }
                else{
                    scm.countdown.text = ""+Math.Ceiling(statusTime + 10 - Time.time);
                }
            }
            // Gives Better You a condition if he loses it from swapping
            else if(board.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.StatusCondition){
                statusCondition = scm.RandomStatusCondition();
                statusTime = Time.time;
                scm.UpdateStatusIcon(statusCondition);
            }
        }

        public void InitManaBar() {
            if (!enabled || !board) {
                Debug.LogWarning("trying to init mana bar while it is disabled");
                return;
            }

            if (!board.Battler) {
                Debug.LogWarning("trying to init mana bar with no battler!");
                return;
            }


            if(board.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.HealingGauge){
                recoveryGaugeText.transform.parent.gameObject.SetActive(true);
                recoveryGaugeAmount = 0;
                recoveryGaugeText.text = ""+recoveryGaugeAmount;
            }
            else{
                recoveryGaugeText.transform.parent.gameObject.SetActive(false);
            }

            // initialize status condition
            if(board.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.StatusCondition){
                statusCondition = scm.RandomStatusCondition();
            }
            else{
                statusCondition = StatusConditions.NoCondition;
            }
            scm.UpdateStatusIcon(statusCondition);
            statusTime = Time.time;
            statusDamageTime = Time.time;

            // don't show mana bar if battler does not use mana.
            if(board.Battler.activeAbilityMana == 0) {
                abilityBarImage.enabled = false;
                return;
            }

            // set height based on mana required for battler - 7px per mana
            // manaBar.sizeDelta = new Vector2(manaBar.sizeDelta.x, board.Battler.activeAbilityMana*7f);
            abilityBarImage.rectTransform.sizeDelta = new Vector2(abilityBarImage.rectTransform.sizeDelta.x, board.Battler.activeAbilityMana*7f);
            abilityBarMaterial.SetFloat("_AspectRatio", abilityBarImage.rectTransform.sizeDelta.x / abilityBarImage.rectTransform.sizeDelta.y);
            abilityBarMaterial.SetFloat("_Flip", board.GetPlayerSide());

            mana = (int) (board.Battler.startAtFullMana ? board.Battler.activeAbilityMana : board.Battler.activeAbilityMana * board.boardStats[ArcadeStats.Stat.StartingSpecial]);

            RefreshManaBar();
        }

        public void RefreshManaBar()
        {        
            if (!enabled) return;
            // if this is disabled, also disable mana bar
            abilityBarMaterial.SetFloat("_CurrentValue", 1f * mana / board.Battler.activeAbilityMana);
            abilityBarMaterial.SetFloat("_Ready", canUseAbility ? 1 : 0);
        }

        public void GainMana(int count)
        {
            if (!enabled || !board || !board.Battler) return;
            if (mana >= board.Battler.activeAbilityMana) return;

            mana = Math.Min(mana+count, board.Battler.activeAbilityMana);

            // mana filled to max, play sound and animation. only run if ability requires mana
            if (mana >= board.Battler.activeAbilityMana && board.Battler.activeAbilityMana > 0)
            {
                Instantiate(manaFillSFX);
                abilityBarImage.GetComponent<ColorFlash>().Flash();
            }

            RefreshManaBar();
        }

        public bool canUseAbility => board.Battler.activeAbilityEffect != Battler.ActiveAbilityEffect.None && mana >= board.Battler.activeAbilityMana;

        public void UseAbility() {
            if (!enabled) return;
            
            if (canUseAbility) {
                mana = 0;
                RefreshManaBar();
                Debug.Log("use active ability");

                board.matchStats.totalAbilityUses++;

                switch (board.Battler.activeAbilityEffect)
                {
                    case Battler.ActiveAbilityEffect.IronSword: IronSword(); break;
                    case Battler.ActiveAbilityEffect.Whirlpool: Whirlpool(); break;
                    case Battler.ActiveAbilityEffect.PyroBomb: PyroBomb(); break;
                    case Battler.ActiveAbilityEffect.Foresight: Foresight(); break;
                    case Battler.ActiveAbilityEffect.GoldMine: GoldMine(); break;
                    case Battler.ActiveAbilityEffect.ZBlind: ZBlind(); break;
                    case Battler.ActiveAbilityEffect.ThunderRush: ThunderRush(); break;
                    case Battler.ActiveAbilityEffect.HeroicShield: HeroicShield(); break;
                    case Battler.ActiveAbilityEffect.Alchemy: Alchemy(); break;
                    case Battler.ActiveAbilityEffect.Swap: Swap(); break;
                    case Battler.ActiveAbilityEffect.Inferno: Inferno(); break;
                    case Battler.ActiveAbilityEffect.FreeMarket: FreeMarket();break;
                    default: break;
                }

                // apply osmose if applicable
                if (board.enemyBoard.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Osmose)
                {
                    // add 10% of max active ability mana to enemy
                    board.enemyBoard.abilityManager.GainMana(board.enemyBoard.Battler.activeAbilityMana / 10);
                }

                // (BANDAID FIX) romra activates their ability in between placing piece and spawning new piece. so dont refresh the ghost tile during the heroic shield ability use
                if (board.Battler.activeAbilityEffect != Battler.ActiveAbilityEffect.HeroicShield) {
                    board.RefreshGhostPiece();
                }
                if (Storage.online && board.netPlayer.isOwned) {
                    board.netPlayer.CmdUseAbility(abilityData);
                }
                Item.Proc(board.equiped, Item.DeferType.OnSpecialUsed);
            }
        }

        /// <summary>
        /// Returns a newly instantiated single piece with a unique ID.
        /// </summary>
        private Piece CreateSinglePiece(bool rotatable) {
            Piece newPiece = Instantiate(singlePiecePrefab).GetComponent<Piece>();
            newPiece.isRotatable = rotatable;
            newPiece.id = board.piecePreview.NextPieceId();
            return newPiece;
        }

        /// <summary>
        /// Piece is replaced with an iron sword that cannot be rotated.
        /// If it is placed or down is pressed, blade quickly shoots through the column and destroys mana in its path.
        /// </summary>
        private void IronSword() {
            ClearAbilityData();
            Piece ironSwordPiece = CreateSinglePiece(false);
            ironSwordPiece.MakeIronSword(board);
            board.ReplacePiece(ironSwordPiece);
        }

        /// <summary>
        /// Sends 3 trash tiles to your opponent's board.
        /// </summary>
        // Treats data as an array of 3 ints of the columns to send each trash tile in.
        private void Whirlpool() {
            // If this is the opponent's client sending a whirlpool,
            // use the retrieved data for columns to send to
            if (Storage.online && !board.netPlayer.isOwned) {
                for (int i=0; i<3; i++) {
                    board.enemyBoard.AddTrashTile(board.rngManager.rng, abilityData[i]);
                }
            } 

            // if not, send in random columns and save the columns sent to
            else {
                if (!board.singlePlayer) abilityData = new int[3];
                
                for (int i=0; i<3; i++) {
                    // in singleplayer, send to own board as score-earning tiles. 
                    // also don't worry abt saving to abilitydata its unused in singleplayer currently
                    if (board.singlePlayer) {
                        board.AddTrashTile(board.rngManager.rng);
                    } else {
                        abilityData[i] = board.enemyBoard.AddTrashTile(board.rngManager.rng);
                    }
                }
            }
        }

        public int PyroMaxDamageDealShield() {
            return 300 + 50 * board.CycleLevel;
        }

        /// <summary>
        /// Replaces current piece and the next 2 in the preview with bombs.
        /// </summary>
        private void PyroBomb() {
            ClearAbilityData();
            board.ReplacePiece(MakePyroBomb());
            board.piecePreview.ReplaceNextPiece(MakePyroBomb());
            board.piecePreview.ReplaceUpcomingPiece(MakePyroBomb(), PiecePreview.previewLength-1);
        }

        private Piece MakePyroBomb() {
            Piece pyroBombPiece = CreateSinglePiece(false);
            pyroBombPiece.MakePyroBomb(board);
            return pyroBombPiece;
        }

        /// <summary>
        /// Gain a foresight symbol, allowing to skip the next unclearable color during a chain.
        /// </summary>
        private void Foresight() {
            ClearAbilityData();
            Instantiate(foresightActivateSFX);
            Instantiate(foresightIconPrefab, symbolList);
        }

        // If this is Psychic and there is a foresight icon available, consume it and return true
        public bool ForesightCheck() {
            return board.Battler.activeAbilityEffect == Battler.ActiveAbilityEffect.Foresight && symbolList.childCount > 0 || board.Battler.activeAbilityEffect == Battler.ActiveAbilityEffect.FreeMarket && symbolList.childCount > 0;
        }

        public void ActivateForesightSkip() {
            // TODO: some kinda particle effect or other visual effect on foresight symbol consumed
            Instantiate(foresightConsumeSFX);
            Destroy(symbolList.GetChild(0).gameObject);
        }

        /// <summary>
        /// Replaces the current piece with a gold mine crystal.
        /// </summary>
        private void GoldMine() {
            ClearAbilityData();
            Piece goldMinePiece = CreateSinglePiece(false);
            goldMinePiece.MakeGoldMine(board);
            board.ReplacePiece(goldMinePiece);
        }

        /// <summary>
        /// Sends a z?man to your opponent's board that obscures the mana color of tiles around it.
        /// After a short duration the tile destroys itself
        /// </summary>
        private void ZBlind() {
            Piece zmanPiece = CreateSinglePiece(false);
            zmanPiece.MakeZman(board);

            if (Storage.online && !board.netPlayer.isOwned) {
                board.enemyBoard.SpawnStandalonePiece(zmanPiece, abilityData[0]);
            } 

            else {
                abilityData = new int[1];
                abilityData[0] = board.enemyBoard.SpawnStandalonePiece(zmanPiece);
            }
        }

        private void ThunderRush(){
            abilityEndTime = Time.time + 10f;
            Instantiate(thunderActivateSFX);
            if(!thunderRushActive){
                Instantiate(thunderIconPrefab, symbolList);
                thunderRushActive = true;
            }
            // thunderIcon.gameObject.SetActive(true);
        }

        private void HeroicShield(){
            Instantiate(heroicShieldSFX);

            // if incoming is empty generate 150 shield
            if (board.totalIncomingDamage == 0) {
                board.AddShield(150 + board.CycleLevel * 25);
                return;
            } 
            // if there is incoming damage send it to the start and counter 150 of it
            else {
                board.hpBar.CounterIncoming(150 + board.CycleLevel * 25);

                for (int i=5; i>=1; i--){
                    board.hpBar.DamageQueue[0].AddDamage(board.hpBar.DamageQueue[i].dmg);
                    board.hpBar.DamageQueue[i].SetDamage(0);
                }
            }

            
        }

        private void Alchemy() {
            ClearAbilityData();
            board.ReplacePiece(MakeBithecaryBomb());
        }

        private Piece MakeBithecaryBomb() {
            Piece potion = CreateSinglePiece(true);
            potion.MakeBithecaryBomb(board);
            return potion;
        }

        public void UpdateHealingGauge() {
            recoveryGaugeText.text = ""+recoveryGaugeAmount;
        }

        // Fills healing gauge based on damage passed (actual amount added is damage/7 as of writing; code in gameobard.cs)
        public void FillHealingGauge(int amount) {
            recoveryGaugeAmount += amount;
            UpdateHealingGauge();
        }

        public void BithecaryHealActivate() {
            board.SetHp(board.hp + recoveryGaugeAmount); // this is basically an uncapped heal but could be fun. if this is too op, change to normal Heal()
            recoveryGaugeAmount = 0;
            UpdateHealingGauge();
        }

        // This is for Better You's ability
        public void Swap(){
            // Swapping cycle position
            int temp = board.cyclePosition;
            board.cyclePosition = board.enemyBoard.cyclePosition;
            board.enemyBoard.cyclePosition = temp;
            Instantiate(heroicShieldSFX);
            board.PointerReposition();
            board.enemyBoard.PointerReposition();

            //Swapping status conditions
            StatusConditions temp2 = statusCondition;
            if(temp2 == StatusConditions.Fire){
                temp2 = StatusConditions.FireSwapped;
                AchievementHandler ah = FindObjectOfType<AchievementHandler>();
                ah.UnlockAchievement("SwapFire");
                ah.UpdateSteamAchievements();
                }
            else if(temp2 == StatusConditions.Poison){temp2 = StatusConditions.PoisonSwapped;}
            statusCondition = board.enemyBoard.abilityManager.statusCondition;
            board.enemyBoard.abilityManager.statusCondition = temp2;
            statusTime = Time.time;
            board.enemyBoard.abilityManager.statusTime = Time.time;
            board.enemyBoard.abilityManager.scm.gameObject.SetActive(true);
            scm.UpdateStatusIcon(statusCondition);
            board.enemyBoard.abilityManager.scm.UpdateStatusIcon(board.enemyBoard.abilityManager.statusCondition);

            Debug.Log("SWAP!!!");
        }


        /// <summary>
        /// Basically Infinity's ability except it has a lower damage mult, leaves a burning fire
        /// for 30 seconds that destroys all pieces that fall on it, and costs more
        /// </summary>
        private void Inferno() {
            ClearAbilityData();
            Piece infernoPiece = CreateSinglePiece(false);
            infernoPiece.MakeInferno(board);
            board.ReplacePiece(infernoPiece);
        }


        // <summary>
        // Xuirbo doesn't play Mana Cycle

        private void FreeMarket() {  // fuckass menu system i rewrote like 5 times to not crash the game, probably not optimally coded but fuck it imma make Xuirbo functional somehow
            if(board.PieceName()=="MainMenu-Invest"){
                selectedStock = 0;
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.investmentIconsJP.SetActive(true);
                    board.xuirboStuff.menuText.text =
                    "株を選んで:\n"+
                    "   1\n"+
                    "   2\n"+
                    "   3\n"+
                    "   4\n"+
                    "   5\n";
                }
                else{
                    board.xuirboStuff.investmentIconsEN.SetActive(true);
                    board.xuirboStuff.menuText.text =
                    "Select investment:\n"+
                    "   1\n"+
                    "   2\n"+
                    "   3\n"+
                    "   4\n"+
                    "   5\n";
                }
                board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Invest-1","[1]"));
            }
            else if(board.PieceName().StartsWith("Invest-")){
                selectedStock = Int32.Parse(board.PieceName().Substring(board.PieceName().Length - 1));
                board.xuirboStuff.investmentIconsEN.SetActive(false);
                board.xuirboStuff.investmentIconsJP.SetActive(false);
                
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.menuText.text =
                    "いくつ買う？\n"+
                    "1株\n"+
                    "5株\n"+
                    "お金の50%\n"+
                    "お金の100%";
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("BuyStock-1","1"));
                }
                else{
                    board.xuirboStuff.menuText.text =
                    "Invest How Much?\n"+
                    "1 stock\n"+
                    "5 stocks\n"+
                    "50% of funds\n"+
                    "100% of funds";
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("BuyStock-1","1"));
                }
            }
            else if(board.PieceName().StartsWith("BuyStock-")){
                XuirboStuff x = board.xuirboStuff;

                if(board.PieceName()=="BuyStock-1"){
                    if(selectedStock == 1){
                        if(x.money >= x.circlePrice){
                            x.money -= x.circlePrice;
                            x.circleStock += x.circlePrice;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 2){
                        if(x.money >= x.triangleUpPrice){
                            x.money -= x.triangleUpPrice;
                            x.triangleUpStock += x.triangleUpPrice;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 3){
                        if(x.money >= x.triangleDownPrice){
                            x.money -= x.triangleDownPrice;
                            x.triangleDownStock += x.triangleDownPrice;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 4){
                        if(x.money >= x.squarePrice){
                            x.money -= x.squarePrice;
                            x.squareStock += x.squarePrice;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 5){
                        if(x.money >= x.diamondPrice){
                            x.money -= x.diamondPrice;
                            x.diamondStock += x.diamondPrice;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                }
                else if(board.PieceName()=="BuyStock-5"){
                    if(selectedStock == 1){
                        if(x.money >= x.circlePrice*5){
                            x.money -= x.circlePrice*5;
                            x.circleStock += x.circlePrice*5;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 2){
                        if(x.money >= x.triangleUpPrice*5){
                            x.money -= x.triangleUpPrice*5;
                            x.triangleUpStock += x.triangleUpPrice*5;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 3){
                        if(x.money >= x.triangleDownPrice*5){
                            x.money -= x.triangleDownPrice*5;
                            x.triangleDownStock += x.triangleDownPrice*5;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 4){
                        if(x.money >= x.squarePrice*5){
                            x.money -= x.squarePrice*5;
                            x.squareStock += x.squarePrice*5;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                    else if(selectedStock == 5){
                        if(x.money >= x.diamondPrice*5){
                            x.money -= x.diamondPrice*5;
                            x.diamondStock += x.diamondPrice*5;
                            Instantiate(board.cosmetics.moneySFX);
                        }
                        else{
                            ShowBadPopup(rejctedCard);
                        }
                    }
                }
                else if(board.PieceName()=="BuyStock-50"){
                    if(selectedStock == 1){
                        x.circleStock += (x.money/2);
                        x.money -= (x.money/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 2){
                        x.triangleUpStock += (x.money/2);
                        x.money -= (x.money/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 3){
                        x.triangleDownStock += (x.money/2);
                        x.money -= (x.money/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 4){
                        x.squareStock += (x.money/2);
                        x.money -= (x.money/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 5){
                        x.diamondStock += (x.money/2);
                        x.money -= (x.money/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                }
                else if(board.PieceName()=="BuyStock-100"){
                    if(selectedStock == 1){
                        x.circleStock += x.money;
                        x.money -= x.money;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 2){
                        x.triangleUpStock += x.money;
                        x.money -= x.money;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 3){
                        x.triangleDownStock += x.money;
                        x.money -= x.money;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 4){
                        x.squareStock += x.money;
                        x.money -= x.money;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 5){
                        x.diamondStock += x.money;
                        x.money -= x.money;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                }

                x.UpdateXuirboText();
                x.menuGameObject.SetActive(false);
                board.DestroyCurrentPiece(); //SpawnPiece();
            }

            else if(board.PieceName()=="MainMenu-Sell"){
                selectedStock = 0;
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.investmentIconsJP.SetActive(true);
                    board.xuirboStuff.menuText.text =
                    "売る株を選んで:\n"+
                    "   1\n"+
                    "   2\n"+
                    "   3\n"+
                    "   4\n"+
                    "   5\n";
                }
                else{
                    board.xuirboStuff.investmentIconsEN.SetActive(true);
                    board.xuirboStuff.menuText.text =
                    "Select stock to sell:\n"+
                    "   1\n"+
                    "   2\n"+
                    "   3\n"+
                    "   4\n"+
                    "   5\n";
                }
                board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Sell-1","[1]"));
                }
            else if(board.PieceName().StartsWith("Sell-")){
                selectedStock = Int32.Parse(board.PieceName().Substring(board.PieceName().Length - 1));
                board.xuirboStuff.investmentIconsJP.SetActive(false);
                board.xuirboStuff.investmentIconsEN.SetActive(false);
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.menuText.text =
                    "いくつ売る？\n"+
                    "株の25%\n"+
                    "株の50%\n"+
                    "株の100%";
                }
                else{
                    board.xuirboStuff.menuText.text =
                    "Sell How Much?\n"+
                    "25% of stock\n"+
                    "50% of stock\n"+
                    "100% of stock";
                }
                board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("SellStock-25","25%"));
            }
            else if(board.PieceName().StartsWith("SellStock-")){
                XuirboStuff x = board.xuirboStuff;
                if(board.PieceName()=="SellStock-25"){
                    if(selectedStock == 1){
                        x.money += (x.circleStock/4);
                        x.circleStock -= (x.circleStock/4);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 2){
                        x.money += (x.triangleUpStock/4);
                        x.triangleUpStock -= (x.triangleUpStock/4);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 3){
                        x.money += (x.triangleDownStock/4);
                        x.triangleDownStock -= (x.triangleDownStock/4);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 4){
                        x.money += (x.squareStock/4);
                        x.squareStock -= (x.squareStock/4);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 5){
                        x.money += (x.diamondStock/4);
                        x.diamondStock -= (x.diamondStock/4);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                }
                else if(board.PieceName()=="SellStock-50"){
                    if(selectedStock == 1){
                        x.money += (x.circleStock/2);
                        x.circleStock -= (x.circleStock/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 2){
                        x.money += (x.triangleUpStock/2);
                        x.triangleUpStock -= (x.triangleUpStock/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 3){
                        x.money += (x.triangleDownStock/2);
                        x.triangleDownStock -= (x.triangleDownStock/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 4){
                        x.money += (x.squareStock/2);
                        x.squareStock -= (x.squareStock/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 5){
                        x.money += (x.diamondStock/2);
                        x.diamondStock -= (x.diamondStock/2);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                }
                if(board.PieceName()=="SellStock-100"){
                    if(selectedStock == 1){
                        x.money += (x.circleStock);
                        x.circleStock -= (x.circleStock);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 2){
                        x.money += (x.triangleUpStock);
                        x.triangleUpStock -= (x.triangleUpStock);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 3){
                        x.money += (x.triangleDownStock);
                        x.triangleDownStock -= (x.triangleDownStock);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 4){
                        x.money += (x.squareStock);
                        x.squareStock -= (x.squareStock);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else if(selectedStock == 5){
                        x.money += (x.diamondStock);
                        x.diamondStock -= (x.diamondStock);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                }
                x.UpdateXuirboText();
                x.menuGameObject.SetActive(false);
                board.DestroyCurrentPiece();
            }
            else if(board.PieceName()=="MainMenu-Shop"){
                selectedStock = 0;
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.menuText.text =
                    "150シールド: $"+(int)(100*board.xuirboStuff.inflation)+"\n"+
                    "餌: $"+(int)(125*board.xuirboStuff.inflation)+"\n" +
                    "ヒーリング: $"+(int)(300*board.xuirboStuff.inflation)+"\n"+
                    "火: $"+(int)(75*board.xuirboStuff.inflation)+"\n"+
                    "爆弾: $"+(int)(230*board.xuirboStuff.inflation)+"\n"
                    ;
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Shop-Shield","シールド"));
                }
                else{
                    board.xuirboStuff.menuText.text =
                    "150 shield: $"+(int)(100*board.xuirboStuff.inflation)+"\n"+
                    "Bait: $"+(int)(125*board.xuirboStuff.inflation)+"\n" +
                    "Healing: $"+(int)(300*board.xuirboStuff.inflation)+"\n"+
                    "Fire: $"+(int)(75*board.xuirboStuff.inflation)+"\n"+
                    "Bombs: $"+(int)(230*board.xuirboStuff.inflation)+"\n"
                    ;
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Shop-Shield","Shield"));
                }
            }
            else if(board.PieceName().StartsWith("Shop-")){
                bool dontDestroy = false;
                if(board.PieceName() == "Shop-Shield"){
                    if(board.xuirboStuff.money >= (int)(100*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(100*board.xuirboStuff.inflation);
                        board.AddShield(150);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Shop-Bait"){
                    if(board.xuirboStuff.money >= (int)(125*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(125*board.xuirboStuff.inflation);
                        board.xuirboStuff.bait += 1;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Shop-Healing"){
                    if(board.xuirboStuff.money >= (int)(125*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(125*board.xuirboStuff.inflation);
                        statusTime = Time.time;
                        statusCondition = StatusConditions.Pure;
                        scm.gameObject.SetActive(true);
                        scm.UpdateStatusIcon(StatusConditions.Pure);
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Shop-Fire"){
                    if(board.xuirboStuff.money >= (int)(75*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(75*board.xuirboStuff.inflation);
                        Piece infernoPiece = CreateSinglePiece(false);
                        infernoPiece.MakeInferno(board);
                        board.ReplacePiece(infernoPiece);
                        dontDestroy = true;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Shop-Bomb"){
                    if(board.xuirboStuff.money >= (int)(230*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(230*board.xuirboStuff.inflation);
                        board.ReplacePiece(MakePyroBomb());
                        board.piecePreview.ReplaceNextPiece(MakePyroBomb());
                        board.piecePreview.ReplaceUpcomingPiece(MakePyroBomb(), PiecePreview.previewLength-1);
                        dontDestroy = true;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                board.xuirboStuff.UpdateXuirboText();
                board.xuirboStuff.menuGameObject.SetActive(false);
                if(!dontDestroy){
                    board.DestroyCurrentPiece();
                }
            }
            else if(board.PieceName()=="MainMenu-Hire"){
                selectedStock = 0;
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.menuText.text =
                    "鉱夫: $"+(int)(125*board.xuirboStuff.inflation)+"\n"+
                    "傭兵: $"+(int)(200*board.xuirboStuff.inflation)+"\n" +
                    "300ダメージ: $"+(int)(200*board.xuirboStuff.inflation)+"\n" +
                    "750ダメージ: $"+(int)(500*board.xuirboStuff.inflation)+"\n"+
                    "放火: $"+(int)(1000*board.xuirboStuff.inflation)+"\n"
                    ;
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Hire-Miner","鉱夫"));
                }
                else{
                    board.xuirboStuff.menuText.text =
                    "Miner: $"+(int)(125*board.xuirboStuff.inflation)+"\n"+
                    "Mercenary: $"+(int)(200*board.xuirboStuff.inflation)+"\n" +
                    "Send 300dmg: $"+(int)(200*board.xuirboStuff.inflation)+"\n" +
                    "Send 750dmg: $"+(int)(500*board.xuirboStuff.inflation)+"\n"+
                    "Arson: $"+(int)(1000*board.xuirboStuff.inflation)+"\n"
                    ;
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Hire-Miner","Miner"));
                }
            }
            else if(board.PieceName().StartsWith("Hire-")){
                if(board.PieceName() == "Hire-Miner"){
                    if(board.xuirboStuff.money >= (int)(125*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(125*board.xuirboStuff.inflation);
                        board.xuirboStuff.miners += 1;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Hire-Mercenary"){
                    if(board.xuirboStuff.money >= (int)(200*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(200*board.xuirboStuff.inflation);
                        board.xuirboStuff.mercenaries += 1;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Hire-Snipe300"){
                    if(board.xuirboStuff.money >= (int)(200*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(200*board.xuirboStuff.inflation);
                        board.EvaluateInstantOutgoingDamage(300);
                        board.xuirboStuff.CrimeChance();
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Hire-Snipe750"){
                    if(board.xuirboStuff.money >= (int)(500*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(500*board.xuirboStuff.inflation);
                        board.EvaluateInstantOutgoingDamage(750);
                        board.xuirboStuff.CrimeChance();
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                else if(board.PieceName() == "Hire-Poison"){
                    if(board.xuirboStuff.money >= (int)(1000*board.xuirboStuff.inflation)){
                        board.xuirboStuff.money -= (int)(1000*board.xuirboStuff.inflation);
                        AbilityManager e = board.enemyBoard.abilityManager;
                        e.statusTime = Time.time;
                        e.statusCondition = StatusConditions.FireSwapped;
                        e.scm.gameObject.SetActive(true);
                        e.scm.UpdateStatusIcon(StatusConditions.FireSwapped);
                        board.xuirboStuff.CrimeChance();
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                board.xuirboStuff.UpdateXuirboText();
                board.xuirboStuff.menuGameObject.SetActive(false);
                board.DestroyCurrentPiece();
            }
            else if(board.PieceName()=="MainMenu-Bribe"){
                if(board.xuirboStuff.policeActive == true){
                    if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                        board.xuirboStuff.menuText.text =
                        "警察に賄賂を贈る：\n"+
                        "$"+(500*board.xuirboStuff.crimes)+"\n";
                        board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Bribe-Pay","払う"));
                    }
                    else{
                        board.xuirboStuff.menuText.text =
                        "Bribe police:\n"+
                        "$"+(500*board.xuirboStuff.crimes)+"\n";
                        board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Bribe-Pay","Pay"));
                    }
                }
                else{
                    if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                        ShowBadPopup("今は賄賂を渡す相手がいないよ");
                    }
                    else{
                        ShowBadPopup("You have no one to bribe right now.");
                    }
                    board.xuirboStuff.UpdateXuirboText();
                    board.xuirboStuff.menuGameObject.SetActive(false);
                    board.DestroyCurrentPiece();
                }
            }
            else if(board.PieceName()=="Bribe-Pay"){
                if(board.xuirboStuff.policeActive == true){
                    if(board.xuirboStuff.money >= 500*board.xuirboStuff.crimes){
                        board.xuirboStuff.money -= 500*board.xuirboStuff.crimes;
                        board.xuirboStuff.crimes = 0;
                        board.xuirboStuff.policePopup.SetActive(false);
                        board.xuirboStuff.policeActive = false;
                        Instantiate(board.cosmetics.moneySFX);
                    }
                    else{
                        ShowBadPopup(rejctedCard);
                    }
                }
                board.xuirboStuff.UpdateXuirboText();
                board.xuirboStuff.menuGameObject.SetActive(false);
                board.DestroyCurrentPiece();
            }
            else if(board.PieceName()=="MainMenu-Fish"){
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.menuText.text =
                    "釣りの時間です...\n\n"+
                    "1つの餌を使って、何かランダムで壮大なものを釣る。"+(500*board.xuirboStuff.crimes)+"\n";
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Fish-Fish","釣り"));
                }
                else{
                    board.xuirboStuff.menuText.text =
                    "It's fishing time...:\n\n"+
                    "Use 1 bait to fish something random and epic."+(500*board.xuirboStuff.crimes)+"\n";
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Fish-Fish","Fish"));
                }
            }
            else if(board.PieceName()=="Fish-Fish"){
                XuirboStuff x = board.xuirboStuff;
                bool dontDelete = false;
                if(x.bait > 0){
                    x.bait -= 1;
                    int f = UnityEngine.Random.Range(0,9);
                    if(f == 0){
                        x.money += 1;
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("あなたはパリッとした1ドルを見つけました!おいしそう!");
                        }
                        else{
                            ShowFishingPopup("You found a crisp $1! Yummers!");
                        }
                    }
                    else if(f == 1){
                        statusTime = Time.time;
                        statusCondition = StatusConditions.Poison;
                        scm.gameObject.SetActive(true);
                        scm.UpdateStatusIcon(StatusConditions.Poison);
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("湖の水を飲んで毒を盛ったんだ!");
                        }
                        else{
                            ShowFishingPopup("You drank lake water and poisoned yourself!");
                        }
                    }
                    else if(f == 2){
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("何も捕まえられなかったが、かっこいいヒラメを見た。");
                        }
                        else{
                            ShowFishingPopup("You didn't catch anything, but saw a cool flounder.");
                        }
                    }
                    else if(f == 3){
                        x.miners += 1;
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("水たまりで掘っている失われた鉱夫を見つけました！");
                        }
                        else{
                            ShowFishingPopup("You found a lost miner digging in a puddle!");
                        }
                    }
                    else if(f == 4){
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("爆弾を見つけた！");
                        }
                        else{
                            ShowFishingPopup("You found a bomb!");
                        }
                        board.ReplacePiece(MakePyroBomb());
                        dontDelete = true;
                    }
                    else if(f == 5 || f == 8){
                        x.flesh += 1;
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("肉の結晶を見つけました！");
                        }
                        else{
                            ShowFishingPopup("You found a FLESH CRYSTAL!");
                        }
                    }
                    else if(f == 6){
                        Instantiate(foresightIconPrefab, symbolList);
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("あなたは波を見つめ、宇宙の秘密を学びました。");
                        }
                        else{
                            ShowFishingPopup("You gazed into the waves and learned the secrets of the universe.");
                        }
                    }
                    else if(f == 7){
                        Piece goldMinePiece = CreateSinglePiece(false);
                        goldMinePiece.MakeGoldMine(board);
                        board.ReplacePiece(goldMinePiece);
                        dontDelete = true;
                        if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                            ShowFishingPopup("宝石を見つけました！");
                        }
                        else{
                            ShowFishingPopup("You found a gem!");
                        }
                    }
                }
                else{
                    if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                        ShowBadPopup("餌が足りない！");
                    }
                    else{
                        ShowBadPopup("Not enough bait!");
                    }
                }
                if(!dontDelete){
                    board.DestroyCurrentPiece();
                }
                board.xuirboStuff.UpdateXuirboText();
                board.xuirboStuff.menuGameObject.SetActive(false);
            }
            else if(board.PieceName()=="MainMenu-Flesh"){
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.menuText.text =
                    "カタクリズムを始めるには、5つのフレッシュクリスタルを使用してください:\n";
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Flesh-Begin","始める"));
                }
                else{
                    board.xuirboStuff.menuText.text =
                    "Use 5 Flesh Crystals to begin the cataclysm:\n";
                    board.ReplacePiece(board.abilityManager.GenerateXuirboMenuPiece("Flesh-Begin","Commence"));
                }
            }
            else if(board.PieceName()=="Flesh-Begin"){
                if(board.xuirboStuff.flesh >= 5){
                    board.xuirboStuff.flesh -= 5;
                    board.xuirboStuff.fleshTimer = Time.time;
                    Instantiate(board.cosmetics.alarmSFX);
                    if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                        board.xuirboStuff.fleshPopupText.text = "それは目覚めました";
                        board.xuirboStuff.fleshPopupText.font = board.xuirboStuff.jpFont;
                    }
                    else{
                        board.xuirboStuff.fleshPopupText.text = "it has awakened";
                        board.xuirboStuff.fleshPopupText.font = board.xuirboStuff.enFontPixel;
                    }
                    board.xuirboStuff.fleshPopup.SetActive(true);
                    
                }
                else{
                    if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                        ShowBadPopup("肉が足りない");
                    }
                    else{
                        ShowBadPopup("you lack flesh");
                    }
                }
                board.DestroyCurrentPiece();
                board.xuirboStuff.UpdateXuirboText();
                board.xuirboStuff.menuGameObject.SetActive(false);
            }

            else{
                ClearAbilityData();
                board.xuirboStuff.menuGameObject.SetActive(true);
                board.xuirboStuff.investmentIconsJP.SetActive(false);
                board.xuirboStuff.investmentIconsEN.SetActive(false);
                if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                    board.xuirboStuff.menuText.font = board.xuirboStuff.jpFont;
                    board.xuirboStuff.menuText.text =
                    "株に投資する\n"+
                    "株を売る\n"+
                    "買い物に行く\n"+
                    "従業員を雇う\n"+
                    "賄賂を贈る\n"+
                    "釣りに行く\n"+
                    "肉体の結晶\n";
                }
                else{
                    board.xuirboStuff.menuText.font = board.xuirboStuff.enFontPixel;
                    board.xuirboStuff.menuText.text =
                    "Invest Assets\n"+
                    "Sell Assets\n"+
                    "Shopping\n"+
                    "Hire Talent\n"+
                    "Bribery\n"+
                    "Go Fishing\n"+
                    "Flesh Crystal\n";
                }
                board.ReplacePiece(MarketMainMenuSelect());
            }
        }
        
        public void ShowFishingPopup(String s)
        {
            XuirboStuff x = board.xuirboStuff;
            x.fishingText.text = s;
            if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                x.fishingText.font = board.xuirboStuff.jpFont;
            }
            else{
                x.fishingText.font = board.xuirboStuff.enFontPixel;
            }
            x.fishingTimer = Time.time;
            x.fishingPopupGameObject.SetActive(true);
            Instantiate(board.cosmetics.fishSFX);
        }


        public void ShowBadPopup(String s)
        {
            XuirboStuff x = board.xuirboStuff;
            x.badText.text = s;
            if(LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ja")){
                x.badText.font = board.xuirboStuff.jpFont;
                if(s == rejctedCard){
                    s = rejctedCardjp;
                }
            }
            else{
                x.badText.font = board.xuirboStuff.enFontPixel;
            }
            x.badPopupTimer = Time.time;
            x.badPopupGameObject.SetActive(true);
            Instantiate(board.cosmetics.declinedSFX);
        }

        public Piece MarketMainMenuSelect() {
            Piece m = CreateSinglePiece(true);
            m.MarketMainMenuSelectInvest(board);
            return m;
        }

        public Piece GenerateXuirboMenuPiece(String centerName, String displayText) {
            Piece m = CreateSinglePiece(true);
            m.CreateXuirboMenuOption(board,centerName,displayText);
            return m;
        }




        public void ClearAbilityData() {
            if (abilityData.Length > 0) abilityData = new int[0];
        }
    }
}