using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackPlayerCards : MonoBehaviour
{
    // Blackjack Objects (in-game displays)
    public BlackjackCardDisplay card1_obj,card2_obj,card3_obj,card4_obj,card5_obj;
    // Blackjack Card Number (as in what card it is)
    public BlackjackCard card1_num,card2_num,card3_num,card4_num,card5_num;

    public void Update(){
        card1_obj.setCard(card1_num);
        card2_obj.setCard(card2_num);
        card3_obj.setCard(card3_num);
        card4_obj.setCard(card4_num);
        card5_obj.setCard(card5_num);
    }

    public int getPlayerSum(){
        int n =
        card1_num.val+
        card2_num.val+
        card3_num.val+
        card4_num.val+
        card5_num.val;
        
        Debug.Log("SUM: "+n+"  | "+ 
        card1_num.val+" + "+
        card2_num.val+" + "+
        card3_num.val+" + "+
        card4_num.val+" + "+
        card5_num.val+" + ");
        
        return n;
    }

}