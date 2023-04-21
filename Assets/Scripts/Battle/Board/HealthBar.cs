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
        [SerializeField] public GameObject hpDisp;
        [SerializeField] private GameObject incomingDmgDisp;

        // cached components o' stuff
        private Image hpImage;
        private Image incomingDmgImage;
        private RectTransform hpTransform;
        private RectTransform incomingDmgTransform;
        private float hpBarTopY;
        [SerializeField] private IncomingDamage[] damageQueue;
        public IncomingDamage[] DamageQueue { get { return damageQueue; } } // (public getter for private setter)
        private float newIncomingPos;

        // Start is called before the first frame update
        void Start()
        {   
            foreach (IncomingDamage incoming in damageQueue)
            {
                incoming.SetDamage(0);
            }
        }

        // Counter the damage in this queue with an incoming damage source.
        // Return the amount of leftover damage.
        public int CounterIncoming(int damage)
        {
            // Iterate in reverse order; target closer daamges first
            for (int i=5; i>=0; i--)
            {
                IncomingDamage incoming = DamageQueue[i];
                // If incoming has equal or more damage to current, put all damage into it and return 0, no more leftover damage
                if (incoming.dmg >= damage)
                {
                    incoming.SubtractDamage(damage);
                    Refresh();
                    return 0;
                }
                // otherwise, cancel out all its damage and move to next
                // (will subtract 0 if empty)
                else {
                    damage -= incoming.dmg;
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
            // get image components to edit attributes 
            hpImage = hpDisp.GetComponent<Image>();
            incomingDmgImage = incomingDmgDisp.GetComponent<Image>();

            // get rect transform to change positions later
            hpTransform = hpDisp.GetComponent<RectTransform>();
            incomingDmgTransform = incomingDmgDisp.GetComponent<RectTransform>();

            hpNum.SetHealth(board.hp);
            hpImage.fillAmount = 1f * board.hp / board.maxHp;

            // incoming amount cannot be greater than hp fill amount 
            incomingDmgImage.fillAmount = Math.Min(1f * TotalIncomingDamage() / board.maxHp, hpImage.fillAmount);

            // set incoming bar y to the top of current hp bar
            hpBarTopY = (hpImage.fillAmount * hpTransform.localScale.y) - hpTransform.localScale.y + 1.0f;

            newIncomingPos = Math.Max(hpBarTopY - incomingDmgImage.fillAmount*incomingDmgTransform.localScale.y, 0);
            incomingDmgTransform.anchoredPosition = new Vector2(incomingDmgTransform.anchoredPosition.x, newIncomingPos);
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

            Refresh();
        }
    }
}