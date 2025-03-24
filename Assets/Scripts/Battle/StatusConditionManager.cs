using System;
using UnityEngine;
using UnityEngine.UI;

using Sound; 
using Animation;
using TMPro;
using Mono.CSharp;
using System.Collections.Generic;

public enum StatusConditions{ // caused by Better You's passive, all last for 30 seconds
        Counter, // 1.2x power when counter-attacking
        Fire,    // Take 5 damage per second
        FireSwapped, // Take 25 damage per second
        Pure,    // Heal 7 HP per second
        Block,   // Reduces damage by 20% before it is added to the queue
        Poison,   // 10 damage is sent to your queue per second
        PoisonSwapped, // 50 damage is sent to your queue per second
        NoCondition
    }

public class StatusConditionManager : MonoBehaviour
{
    [SerializeField] private Image StatusIcon;
    public TMP_Text countdown;
    public GameObject extraText;
    [SerializeField] private Sprite Counter,Pure,Block,Fire,Poison;

    public void UpdateStatusIcon(StatusConditions c){
        if(c == StatusConditions.NoCondition){
            gameObject.SetActive(false);
        }
        else{
            gameObject.SetActive(true);
            
            if(c == StatusConditions.FireSwapped || c == StatusConditions.PoisonSwapped){
                extraText.SetActive(true);
            }
            else{
                extraText.SetActive(false);
            }

            if(c == StatusConditions.Pure){
                StatusIcon.sprite = Pure;
            }
            else if(c == StatusConditions.Counter){
                StatusIcon.sprite = Counter;
            }
            else if(c == StatusConditions.Block){
                StatusIcon.sprite = Block;
            }
            else if(c == StatusConditions.Fire || c == StatusConditions.FireSwapped){
                StatusIcon.sprite = Fire;
            }
            else{
                StatusIcon.sprite = Poison;
            }
        }
    }

    public StatusConditions RandomStatusCondition(){
        StatusConditions[] temp = new StatusConditions[]{StatusConditions.Counter,StatusConditions.Block,StatusConditions.Fire,StatusConditions.Poison,StatusConditions.Pure};
        // StatusConditions[] temp = new StatusConditions[]{StatusConditions.Counter,StatusConditions.Block,StatusConditions.Fire};
        int n = UnityEngine.Random.Range(0,temp.Length);
        Debug.Log("Chose random condition " + n);
        return temp[n];
    }
}
