using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Battle {
    [CreateAssetMenu(fileName = "Battler", menuName = "ManaCycle/Battler")]
    public class Battler : ScriptableObject {
        [SerializeField] private string _battlerId;
        public string battlerId => _battlerId;

        [SerializeField] private LocalizedString displayNameEntry;
        public string displayName {get; private set;}

        private void OnEnable() {
            displayNameEntry.StringChanged += UpdateName;
        }

        private void OnDisable() {
            displayNameEntry.StringChanged -= UpdateName;
        }

        // To be run when the name language string needs to be updated
        private void UpdateName(string localizedName) {
            displayName = localizedName;
        }

        [SerializeField] public Sprite sprite;

        /// <summary>Offset of the portrait in the battle view</summary>
        [SerializeField] public Vector2 portraitOffset;

        /// <summary>Effect of the passive ability (if it's not a piece RNG)</summary>
        [SerializeField] public PassiveAbilityEffect passiveAbilityEffect;
        public enum PassiveAbilityEffect
        {
            None,
            Shields, // summon shields if damage bar is empty
            Shapeshifter, // used for the Mirrored, copies other battler's look
            Osmose, // gain special when opponent uses ability
            Instadrop, // trainbot's definitely not overpowered ability
            LastStand, // Geo's revenge mechanic
        }

        /// <summary>Description this battler's passive ability</summary>
        [SerializeField] public string passiveAbilityDesc;

        /// <summary>Effect of the active ability</summary>
        [SerializeField] public ActiveAbilityEffect activeAbilityEffect;
        public enum ActiveAbilityEffect
        {
            None,
            IronSword,
            Whirlpool,
            PyroBomb,
            Foresight,
            GoldMine,
            ZBlind
        }

        /// <summary>If true, this battler will start at full mana</summary>
        public bool startAtFullMana;
            
        /// <summary>Name of this character's active ability</summary>
        [SerializeField] public string activeAbilityName;
        /// <summary>Describes this battler's active ability</summary>
        [SerializeField] public string activeAbilityDesc;
        /// <summary>Describes this character's ability in singleplayer mode, if it is different. Blank for same desc</summary>
        [SerializeField] public string soloAbilityDesc;
        /// <summary>Amount of mana required to use active ability</summary>
        [SerializeField] public int activeAbilityMana;
        /// <summary>The piece RNG used for this battler</summary>
        [SerializeField] public PieceRng pieceRng;

        [SerializeField] public GameObject voiceSFX;

        // used for the attack popup gradients
        [SerializeField] public Material gradientMat;
        [SerializeField] public Color textBoxColor;
    }

    public enum PieceRng {
        Bag, // default
        CurrentColorWeighted, // Infinity
        PieceSameColorWeighted, // Aqua
        CenterMatchesCycle, // Psychic
        PureRandom, // Trainbot
    }
}