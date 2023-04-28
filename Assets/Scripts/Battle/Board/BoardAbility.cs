using UnityEngine;

namespace Battle.Board {
    /// <summary>
    /// Handles the mana bar and abilities for the GameBoard. Party/solo mode only
    /// </summary>
    public class BoardAbility : MonoBehaviour {
        // cached GameBoard
        private GameBoard board;

        [SerializeField] public GameObject manaDisp;

        /// <summary>Current amount of mana the player has generated</summary>
        public int mana {get; private set;}

        void OnValidate() {
            board = GetComponent<GameBoard>();
        }
    }
}