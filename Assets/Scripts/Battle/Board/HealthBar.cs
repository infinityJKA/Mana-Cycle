using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Battle.Board {
    public class HealthBar : MonoBehaviour
    {
        // The board this HP bar is for
        private GameBoard board;

        [SerializeField] public HpNum hpNum;

        // cached components o' stuff
        [SerializeField]
        private Image hpImage;
        [SerializeField]
        private Image incomingDmgImage;
        [SerializeField]
        private Image shieldImage;

        [SerializeField] private Color incomingStartColor, incomingTargetColor, incomingEndColor;

        // used for segmented dmg bar
        private List<GameObject> incDmgBarList = new List<GameObject>();
        
        [SerializeField] private IncomingDamage[] damageQueue;
        public IncomingDamage[] DamageQueue { get { return damageQueue; } } // (public getter for private setter)
        private float newIncomingPos;

        // Shield object to enable while this battler has any shield
        public GameObject shieldObject;
        // The number displaying the shield hp amount
        public TMPro.TextMeshProUGUI shieldNumText;

        // enable/disable for actual game functionality
        [SerializeField] private bool displayOnly = false;

        // Start is called before the first frame update
        void Start()
        {   
            if (displayOnly) return;
            incDmgBarList = new List<GameObject>();

            foreach (IncomingDamage incoming in damageQueue)
            {
                incoming.SetDamage(0);

                // Debug.Log("oq3wjinriow3nouiwe");
                // Debug.Log(incomingDmgImage.gameObject);
                // Debug.Log(transform);
                // Debug.Log(incDmgBarList);
                // create a new gameobject for each damage slot to have segmented hp bar
                if (incomingDmgImage != null ) incDmgBarList.Add(Instantiate(incomingDmgImage.gameObject, hpImage.gameObject.transform));

            }

            if (incomingDmgImage != null) incomingDmgImage.gameObject.SetActive(false);
        }

        // Counter the damage in this queue with an incoming damage source.
        // Return the amount of leftover damage.
        public int CounterIncoming(int damage)
        {
            // romra +30% damage when defending
            if (board.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Defender) {
                damage = (int)(damage * 1.3f);
            }

            // Iterate in reverse order; target closer daamges first
            for (int i=5; i>=0; i--)
            {
                IncomingDamage incoming = DamageQueue[i];
                // If incoming has equal or more damage to current, put all damage into it and return 0, no more leftover damage
                if (incoming.dmg >= damage)
                {
                    incoming.SubtractDamage(damage);
                    board.matchStats.totalDamageCountered += damage;
                    Refresh();
                    return 0;
                }
                // otherwise, cancel out all its damage and move to next
                // (will subtract 0 if empty)
                else {
                    damage -= incoming.dmg;
                    board.matchStats.totalDamageCountered += incoming.dmg;
                    incoming.SetDamage(0);
                }
            }

            // return any leftover damage that will be sent to oppnent
            Refresh();
            return damage;
        }

        // Initializes this HP bar for the GameBoard passed in which this hp bar is for.
        public void Setup(GameBoard board)
        {
            this.board = board;
            Refresh();
        }

        public void Refresh()
        {
            // if no board setup, use storage.cs hp value
            if (board != null) hpImage.fillAmount = 1f * board.hp / board.maxHp;
            else hpImage.fillAmount = 1f * Storage.hp / ArcadeStats.maxHp;

            if (!board) return;
            if (hpNum != null) hpNum.SetHealth(board.hp);

            if (displayOnly) return;

            // set inc damage bar amounts
            for (int i = 0; i < incDmgBarList.Count; i++)
            {
                GameObject barObj = incDmgBarList[i];
                Image barImg = barObj.GetComponent<Image>();

                // get sum of all damage slots before this so later damage is shown on top.
                int newDmg = 0;
                for(int j = i; j < DamageQueue.Length; j++)
                {
                    newDmg += damageQueue[j].dmg;
                    // Debug.Log(damageQueue[j].dmg);
                }

                // set fill amount
                barImg.fillAmount = Math.Min(1f * newDmg / board.maxHp, hpImage.fillAmount);


                // incomingDmgImage.fillAmount = Math.Min(1f * TotalIncomingDamage() / board.maxHp, hpImage.fillAmount);
                float hpBarTopY = (hpImage.fillAmount * hpImage.rectTransform.localScale.y) - hpImage.rectTransform.localScale.y;
                // Debug.Log(hpBarTopY);
                newIncomingPos = hpBarTopY * barObj.GetComponent<RectTransform>().rect.height;
                barImg.rectTransform.anchoredPosition = new Vector2(incomingDmgImage.rectTransform.anchoredPosition.x, newIncomingPos);

                // dont allow fill amount overflow
                barImg.fillAmount = Math.Min(barImg.fillAmount, hpImage.fillAmount);
                

                // set color
                if (i==5) {
                    barImg.color = incomingEndColor;
                } else {
                    barImg.color = Color.Lerp(incomingStartColor, incomingTargetColor, i/4f);
                }

                // Debug.Log(barObj);
            }


            shieldImage.fillAmount = Math.Min(1f * board.shield / board.maxHp, hpImage.fillAmount);
        }

        public int TotalIncomingDamage()
        {
            int total = 0;
            foreach (IncomingDamage incoming in damageQueue)
            {
                total += incoming.dmg;
            }
            return total;
        }

        // Increased by 1 each time the damage advances (including while empty)
        public int damageQueueIndex {get; private set;} = 0;
        public void AdvanceDamageQueue()
        {
            // Advance the incoming damage cycle
            for (int i = 5; i >= 1; i--)
            {
                var prev = damageQueue[i-1];
                damageQueue[i].SetDamage(prev.dmg);

                // sets the text's position to previous, to create a (kind of) seamless animation between them
                damageQueue[i].textComponent.transform.position = prev.textComponent.transform.position;
            }

            damageQueue[0].SetDamage(0);

            damageQueueIndex++;

            Refresh();
        }
    }
}