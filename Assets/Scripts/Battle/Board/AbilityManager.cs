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

        /// <summary>Background image for the mana (MP) bar </summary>
        [SerializeField] public RectTransform manaBar; 
        /// <summary>Fill image for the mana (MP) bar </summary>
        [SerializeField] public Image manaDisp;
        
        [SerializeField] public GameObject singlePiecePrefab;

        // Symbol list that appears near the cycle, used by Psychic's Foresight
        [SerializeField] public Transform symbolList;

        // Psychic's foresight icon prefab
        [SerializeField] public GameObject foresightIconPrefab;

        [SerializeField] private AudioClip manaFillSFX;
        [SerializeField] private AudioClip foresightSFX;

        /// <summary>Current amount of mana the player has generated</summary>
        public int mana {get; private set;}

        /// <summary>
        /// True while the battler's ability is active.
        /// </summary>
        public bool abilityActive;

        void Start()
        {
            mana = 0;
            mana = board.Battler.activeAbilityMana -1; // for easy debug

            RefreshManaBar();
        }

        public void InitManaBar() {
            // set height based on mana required for battler - 50 is the reference
            manaBar.sizeDelta = new Vector2(manaBar.sizeDelta.x, manaBar.sizeDelta.y * board.Battler.activeAbilityMana/50f);
        }

        public void RefreshManaBar()
        {        
            manaDisp.fillAmount = 1f * mana / board.Battler.activeAbilityMana;
        }

        public void GainMana(int count)
        {
            if (mana >= board.Battler.activeAbilityMana) return;

            mana = Math.Min(mana+count, board.Battler.activeAbilityMana);
            // mana filled to max, play sound and animation
            if (mana >= board.Battler.activeAbilityMana)
            {
                SoundManager.Instance.PlaySound(manaFillSFX);
                manaDisp.GetComponent<ColorFlash>().Flash();
            }
            RefreshManaBar();
        }

        public void UseAbility() {
            if (mana >= board.Battler.activeAbilityMana) {
                mana = 0;
                RefreshManaBar();
                Debug.Log("use active ability");
                abilityActive = true;

                switch (board.Battler.activeAbilityEffect)
                {
                    case Battler.ActiveAbilityEffect.IronSword: IronSword(); break;
                    case Battler.ActiveAbilityEffect.Whirlpool: Whirlpool(); break;
                    case Battler.ActiveAbilityEffect.PyroBomb: PyroBomb(); break;
                    case Battler.ActiveAbilityEffect.Foresight: Foresight(); break;
                    case Battler.ActiveAbilityEffect.GoldMine: GoldMine(); break;
                    case Battler.ActiveAbilityEffect.ZBlind: ZBlind(); break;
                    default: break;
                }

                // apply osmose if applicable
                if (board.enemyBoard.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Osmose)
                {
                    // add 10% of max active ability mana to enemy
                    board.enemyBoard.abilityManager.GainMana(board.enemyBoard.Battler.activeAbilityMana / 10);
                }
            }
        }

        /// <summary>
        /// Piece is replaced with an iron sword that cannot be rotated.
        /// If it is placed or down is pressed, blade quickly shoots through the column and destroys mana in its path.
        /// </summary>
        private void IronSword() {
            SinglePiece ironSwordPiece = Instantiate(singlePiecePrefab).GetComponent<SinglePiece>();
            ironSwordPiece.MakeIronSword(board);
            board.ReplacePiece(ironSwordPiece);
        }

        void OnValidate() {
            board = GetComponent<GameBoard>();
        }

        /// <summary>
        /// Sends 3 trash tiles to your opponent's board.
        /// </summary>
        private void Whirlpool() {
            for (int i=0; i<3; i++) board.enemyBoard.AddTrashTile();
        }

        /// <summary>
        /// Replaces current piece and the next 2 in the preview with bombs.
        /// </summary>
        private void PyroBomb() {
            board.ReplacePiece(MakePyroBomb());
            board.piecePreview.ReplaceNextPiece(MakePyroBomb());
            board.piecePreview.ReplaceListPiece(MakePyroBomb(), PiecePreview.previewLength-1);
        }

        private SinglePiece MakePyroBomb() {
            SinglePiece pyroBombPiece = Instantiate(singlePiecePrefab).GetComponent<SinglePiece>();
            pyroBombPiece.MakePyroBomb(board);
            return pyroBombPiece;
        }

        /// <summary>
        /// Gain a foresight symbol, allowing to skip the next unclearable color during a chain.
        /// </summary>
        private void Foresight() {
            SoundManager.Instance.PlaySound(foresightSFX);
            Instantiate(foresightIconPrefab, symbolList);
        }

        // If this is Psychic and there is a foresight icon available, consume it and return true
        public bool ForesightCheck() {
            return (board.Battler.activeAbilityEffect == Battler.ActiveAbilityEffect.Foresight && symbolList.childCount > 0);
        }

        public void UseForesight() {
            // TODO: some kinda particle effect or other visual effect on foresight symbol consumed
            Destroy(symbolList.GetChild(0).gameObject);
        }

        /// <summary>
        /// Replaces the current piece with a gold mine crystal.
        /// </summary>
        private void GoldMine() {
            SinglePiece goldMinePiece = Instantiate(singlePiecePrefab).GetComponent<SinglePiece>();
            goldMinePiece.MakeGoldMine(board);
            board.ReplacePiece(goldMinePiece);
        }

        /// <summary>
        /// Sends a z?man to your opponent's board that obscures the mana color of tiles around it.
        /// After a short duration the tile destroys itself
        /// </summary>
        private void ZBlind() {
            SinglePiece zmanPiece = Instantiate(singlePiecePrefab).GetComponent<SinglePiece>();
            zmanPiece.MakeZman(board);
            board.enemyBoard.SpawnStandalonePiece(zmanPiece);
        }
    }
}