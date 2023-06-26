using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackCardDisplay : MonoBehaviour
{
    public BlackjackCard bj_card;
    public Image spriteCard;
    public int card_val;

    public void setCard(BlackjackCard bj){
        bj_card = bj;
        card_val = bj.val;
    }

    void Update(){
        card_val = bj_card.val;
        if (card_val == 0){gameObject.GetComponent<Image>().enabled = false;}
        else
            {gameObject.GetComponent<Image>().enabled = true;
            spriteCard.sprite = bj_card.cardSprite;}
    }
    
}
