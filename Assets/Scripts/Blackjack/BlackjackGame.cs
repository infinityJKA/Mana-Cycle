using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;


namespace MainMenu {
    public class BlackjackGame : MonoBehaviour
    {
        [SerializeField] GameObject StartMenu;
        [SerializeField] GameObject StartMenuSelected;
        [SerializeField] private BlackjackCard[] BaseDeck;
        public List<BlackjackCard> CurrentDeck;
        public List<BlackjackCard> PlayerHand;
        public List<BlackjackCard> AIHand;
        [SerializeField] GameObject ActionButtons;
        [SerializeField] GameObject ActionSelected;
        [SerializeField] GameObject FinishButtons;
        [SerializeField] GameObject FinishSelected;
        [SerializeField] TMPro.TextMeshProUGUI WinText;
        [SerializeField] TMPro.TextMeshProUGUI WinSubtext;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private BlackjackCard EmptyCard;
        [SerializeField] private BlackjackPlayerCards PlayerCards;
        [SerializeField] private BlackjackPlayerCards AICards;
        [SerializeField] bool mobile;
        [SerializeField] GameObject Geo;
        private bj_cond cond;
        private enum bj_cond{
            MIDGAME,
            WIN,
            LOSE
        }

        void OnEnable(){
            NewGame();
        }

        public void NewGame(){
            Debug.Log("NEW GAME");

            // Disable finished game buttons
            FinishButtons.SetActive(false);

            // Resets hands
            PlayerHand.Clear();
            PlayerCards.card1_num = EmptyCard;
            PlayerCards.card2_num = EmptyCard;
            PlayerCards.card3_num = EmptyCard;
            PlayerCards.card4_num = EmptyCard;
            PlayerCards.card5_num = EmptyCard;
            AIHand.Clear();
            AICards.card1_num = EmptyCard;
            AICards.card2_num = EmptyCard;
            AICards.card3_num = EmptyCard;
            AICards.card4_num = EmptyCard;
            AICards.card5_num = EmptyCard;

            // Resets deck
            CurrentDeck.Clear();
            for(int i=0; i<52; i++){
                CurrentDeck.Add(BaseDeck[i]);
            }

            // Starts the game
            cond = bj_cond.MIDGAME;
            DrawCard(true);
            DrawCard(true);
            
            // Check if over 21
            if(PlayerCards.getPlayerSum() > 21){
                Lose("SPAWN BUST!");
            }
            
            // AI draws, check if lose (possible because of the 999 BILLION card [that is actually only 999 bc integer limit])
            // else give controls to start drawing more cards
            if(cond == bj_cond.MIDGAME){
                AI_DrawCard();
                if(AICards.getPlayerSum() > 21){
                    Win("SPAWN WIN!!!");
                }else{
                    ActionButtons.SetActive(true);
                    if (!mobile) EventSystem.current.SetSelectedGameObject(ActionSelected);}
            }
        }

        public void DrawCard(bool gameStart){
            EventSystem.current.SetSelectedGameObject(null);
            Geo.GetComponent<Animator>().Play("Draw");
            // Adds card to hand and removes from deck
            System.Random r = new System.Random();
            int n = r.Next(1,CurrentDeck.Count);
            PlayerHand.Add(CurrentDeck[n-1]);
            CurrentDeck.RemoveAt(n);

            // Updates Graphics and Num
            if(PlayerHand.Count == 1){
                PlayerCards.card1_num = PlayerHand[0];
            }else if(PlayerHand.Count == 2){
                PlayerCards.card2_num = PlayerHand[1];
            }else if(PlayerHand.Count == 3){
                PlayerCards.card3_num = PlayerHand[2];
            }else if(PlayerHand.Count == 4){
                PlayerCards.card4_num = PlayerHand[3];    
            }else{
                PlayerCards.card5_num = PlayerHand[4];
            }

            // Check if over 21, else check if 5-card rule
            if(PlayerCards.getPlayerSum() > 21){
                Lose("Bust");
            }else if(PlayerHand.Count >= 5){
                Win("5-Card Rule");
            }else if(!gameStart && cond == bj_cond.MIDGAME){
                EventSystem.current.SetSelectedGameObject(ActionSelected);
            }

        }

        private void AI_DrawCard(){
            // Adds card to hand and removes from deck
            System.Random r = new System.Random();
            int n = r.Next(1,CurrentDeck.Count);
            AIHand.Add(CurrentDeck[n-1]);
            CurrentDeck.RemoveAt(n);

            // Updates Graphics
            if(AIHand.Count == 1){
                AICards.card1_num = AIHand[0];
            }else if(AIHand.Count == 2){
                AICards.card2_num = AIHand[1];
            }else if(AIHand.Count == 3){
                AICards.card3_num = AIHand[2];
            }else if(AIHand.Count == 4){
                AICards.card4_num = AIHand[3];    
            }else{
                AICards.card5_num = AIHand[4];
            }

            // Check if over 21
            if(AICards.getPlayerSum() > 21){
                Win("Dealer Bust");
            }else if(AIHand.Count >= 5){
                Lose("5-Card Rule");
            }

        }

        public void BJ_Stand(){
            // Remove controls
            EventSystem.current.SetSelectedGameObject(null);
            ActionButtons.SetActive(false);
            
            // AI draws cards
            if(AICards.getPlayerSum() < 17){
                while(AICards.getPlayerSum() < 17 && AIHand.Count < 5){
                    AI_DrawCard();
                }
            }

            // Win by amount if not won already
            if (cond == bj_cond.MIDGAME){
                if(PlayerCards.getPlayerSum() > AICards.getPlayerSum()){
                    Win("Bigger Hand");
                }else if(PlayerCards.getPlayerSum() == AICards.getPlayerSum()){
                    Win("Via Tie");
                }else{Lose("Smaller Hand");}
            }
        }
        
        private void Lose(string subtext){
            ActionButtons.SetActive(false);
            cond = bj_cond.LOSE;
            WinText.text = "GAME OVER";
            WinSubtext.text = subtext;
            FinishButtons.SetActive(true);
            if (!mobile) EventSystem.current.SetSelectedGameObject(FinishSelected);
        }

        public void Win(string subtext){
            ActionButtons.SetActive(false);
            cond = bj_cond.WIN;
            WinText.text = "YOU WIN!";
            WinSubtext.text = subtext;
            FinishButtons.SetActive(true);
            if (!mobile) EventSystem.current.SetSelectedGameObject(FinishSelected);
        }

        public void Leave(){
            gameObject.SetActive(false);
            StartMenu.SetActive(true);
            if (!mobile) EventSystem.current.SetSelectedGameObject(StartMenuSelected);
            Geo.GetComponent<Animator>().CrossFade("Hands Together",0.15f);
        }

    }
}