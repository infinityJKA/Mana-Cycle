using UnityEngine;

using SoloMode;
using UnityEngine.Localization;
using System;

namespace ConvoSystem {
    [Serializable]
    public class MidLevelConversation {
        
        [Tooltip("All conditions that need to be met to show this convo")]
        [SerializeField] public Objective[] appearConditions;

        [Tooltip("ID of tutorial mask shown. 0 for full dim, -1 for no dim")]
        [SerializeField] public int tutorialMaskID = -1;

        /** Conversation that happens before this level. if null, convo hasn't been loaded yet */
        public Conversation conversation {get; private set;}

        [SerializeField] private LocalizedAsset<Conversation> conversationEntry;

        public void LoadConvo() {
            if (conversationEntry == null) {
                Debug.LogError(this+" (midlevelconvo) has no cutscene entry set");
                return;
            }

            Debug.Log("loading localized cutscene with key "+conversationEntry.TableEntryReference.Key);
            conversationEntry.LoadAssetAsync();
            conversationEntry.AssetChanged += UpdateConversationLocale;
        }

        public void UnloadConvo() {
            conversationEntry.AssetChanged -= UpdateConversationLocale;
        }

        // To be run when the name language string needs to be updated
        private void UpdateConversationLocale(Conversation localizedConvo) {
            // if (localizedConvo == null) {
            //     Debug.LogError(levelName+" has no localized cutscene for current language");
            // }

            conversation = localizedConvo;
        }

        public bool ShouldAppear(Battle.Board.GameBoard board) {
            foreach (Objective condition in appearConditions) {
                if (!condition.IsCompleted(board)) return false;
            }
            return true;
        }
    }
}