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

        [SerializeField] public Sprite sprite;
        [field: SerializeField] public Material material {get; private set;}

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
            UnstableMana, // Electro's blob bonus
            Defender, // Romra's ability
            HealingGauge, // Bithecary's ability
            StatusCondition, // Better You's ability
            FlareBlade, // Erif's ability
            Economics // Xuirbo's ability
        }

        [SerializeField] private LocalizedString passiveAbilityDescEntry;
        /// <summary>Description this battler's passive ability</summary>
        public string passiveAbilityDesc {get; private set;}

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
            ZBlind,
            ThunderRush,
            HeroicShield,
            Alchemy,
            Swap,
            Inferno,
            FreeMarket
        }

        /// <summary>If true, this battler will start at full mana</summary>
        public bool startAtFullMana;
            
        [SerializeField] public LocalizedString activeAbilityNameEntry;
        /// <summary>Name of this character's active ability</summary>
        public string activeAbilityName {get; private set;}


        [SerializeField] private LocalizedString activeAbilityDescEntry;
        /// <summary>Describes this battler's active ability</summary>
        public string activeAbilityDesc {get; private set;}


        [SerializeField] private LocalizedString soloAbilityDescEntry;
        /// <summary>Describes this character's ability in singleplayer mode, if it is different. Blank for same desc</summary>
        public string soloAbilityDesc {get; private set;}


        /// <summary>Amount of mana required to use active ability</summary>
        [SerializeField] public int activeAbilityMana;
        /// <summary>The piece RNG used for this battler</summary>
        [SerializeField] public PieceRng pieceRng;

        [SerializeField] public GameObject voiceSFX;

        // used for the attack popup gradients
        [SerializeField] public Material gradientMat;
        [SerializeField] public Color textBoxColor;
        [SerializeField] public Sprite gameLogo;

        private void OnEnable() {
            // i wish there was an easier way to do this
            // there probably is but whatever
            if (!displayNameEntry.IsEmpty) {
                displayNameEntry.GetLocalizedStringAsync();
                displayNameEntry.StringChanged += UpdateName;
            }

            if (!passiveAbilityDescEntry.IsEmpty) {
                passiveAbilityDescEntry.GetLocalizedStringAsync();
                passiveAbilityDescEntry.StringChanged += UpdatePassiveDesc;
            }

            if (!activeAbilityNameEntry.IsEmpty) {
                activeAbilityNameEntry.GetLocalizedStringAsync();
                activeAbilityNameEntry.StringChanged += UpdateActiveName;

                activeAbilityDescEntry.GetLocalizedStringAsync();
                activeAbilityDescEntry.StringChanged += UpdateActiveDesc;
            }

            if (!soloAbilityDescEntry.IsEmpty) {
                soloAbilityDescEntry.GetLocalizedStringAsync();
                soloAbilityDescEntry.StringChanged += UpdateSoloDesc;
            }
        }

        private void OnDisable() {
            displayNameEntry.StringChanged -= UpdateName;
            passiveAbilityDescEntry.StringChanged -= UpdatePassiveDesc;
            activeAbilityNameEntry.StringChanged -= UpdateActiveName;
            activeAbilityDescEntry.StringChanged -= UpdateActiveDesc;
            soloAbilityDescEntry.StringChanged -= UpdateSoloDesc;
        }

        // To be run when the name language string needs to be updated
        private void UpdateName(string str) {
            displayName = str;
        }

        private void UpdatePassiveDesc(string str) {
            passiveAbilityDesc = str;
        }

        private void UpdateActiveName(string str) {
            activeAbilityName = str;
        }

        private void UpdateActiveDesc(string str) {
            activeAbilityDesc = str;
        }

        private void UpdateSoloDesc(string str) {
            soloAbilityDesc = str;
        }
    }

    public enum PieceRng {
        Bag, // default
        CurrentColorWeighted, // Infinity
        PieceSameColorWeighted, // Aqua
        CenterMatchesCycle, // Psychic
        PureRandom, // Trainbot
    }
}