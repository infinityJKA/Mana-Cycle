using System;
using UnityEngine;
using UnityEngine.UI;

using Sound; 
using Animation;

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

        /// <summary>
        /// True while the battler's ability is active.
        /// </summary>
        public bool abilityActive;

        // used for synchronizing ability with the opponent's client.
        public int[] abilityData;

        // Timer for when to stop Electro's ability
        private float abilityEndTime;
        public bool thunderRushActive = false;
        [SerializeField] public Transform thunderIconPrefab;
        [SerializeField] private GameObject thunderActivateSFX,heroicShieldSFX;

        public int recoveryGaugeAmount;
        private bool alchemyIsHealing;


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
        }

        public void InitManaBar() {
            if (!enabled || !board || !board.Battler) return;

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
                abilityActive = true;

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
                    default: break;
                }

                // apply osmose if applicable
                if (board.enemyBoard.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Osmose)
                {
                    // add 10% of max active ability mana to enemy
                    board.enemyBoard.abilityManager.GainMana(board.enemyBoard.Battler.activeAbilityMana / 10);
                }

                board.RefreshGhostPiece();
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
            return board.Battler.activeAbilityEffect == Battler.ActiveAbilityEffect.Foresight && symbolList.childCount > 0;
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
            abilityEndTime = Time.time + 13;
            Instantiate(thunderActivateSFX);
            if(!thunderRushActive){
                Instantiate(thunderIconPrefab, symbolList);
                thunderRushActive = true;
            }
            // thunderIcon.gameObject.SetActive(true);
        }

        private void HeroicShield(){
            Instantiate(heroicShieldSFX);
            for (int i=5; i>=1; i--){
                board.hpBar.DamageQueue[0].AddDamage(board.hpBar.DamageQueue[i].dmg);
                board.hpBar.DamageQueue[i].SetDamage(0);
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


        public void ClearAbilityData() {
            if (abilityData.Length > 0) abilityData = new int[0];
        }
    }
}