using System;
using UnityEngine;

namespace Battle.Board {
    /// <summary>
    /// Handles the mana bar and abilities for the GameBoard. Party/solo mode only
    /// </summary>
    public class AbilityManager : MonoBehaviour {
        // cached GameBoard
        private GameBoard board;

        /// <summary>Fill image for the mana (MP) bar </summary>
        [SerializeField] public UnityEngine.UI.Image manaDisp;
        
        [SerializeField] public GameObject singlePiecePrefab;

        /// <summary>Current amount of mana the player has generated</summary>
        public int mana {get; private set;}

        /// <summary>
        /// True while the battler's ability is active.
        /// </summary>
        public bool abilityActive;

        void Start()
        {
            mana = 0;
            // mana = board.Battler.activeAbilityMana; // for easy debug
            RefreshManaBar();
        }

        public void RefreshManaBar()
        {            
            manaDisp.fillAmount = 1f * mana / board.Battler.activeAbilityMana;
        }

        public void GainMana(int count)
        {
            mana = Math.Min(mana+count, board.Battler.activeAbilityMana);
            RefreshManaBar();
            Debug.Log("gained "+count+" mana");
        }

        public void UseAbility() {
            if (mana >= board.Battler.activeAbilityMana) {
                mana = 0;
                RefreshManaBar();
                Debug.Log("use active ability");
                abilityActive = true;

                if (board.Battler.activeAbilityName == "Iron Sword") IronSword();
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
    }
}