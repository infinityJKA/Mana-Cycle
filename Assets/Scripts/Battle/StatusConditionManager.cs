using System;
using UnityEngine;
using UnityEngine.UI;

using Sound; 
using Animation;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using Mono.CSharp;
using System.Collections.Generic;

public enum StatusConditions{ // caused by Better You's passive, all last for 30 seconds
        Counter, // Counter up to 300 damage when something enters your queue and then end condition
        Fire,    // Take 5 damage per second, 15 damage per second if swapped. Instantly ends if you pass the end of the mana cycle.
        FireSwapped,
        Pure,    // Heal 5 HP per second
        Block,   // Incoming damage is reduced by 0.8x before put in queue
        Poison,   // 10 damage is sent to your queue per second, 20 damage per second if swapped. Instantly ends if you pass the end of the mana cycle.
        PoisonSwapped,
        NoCondition
    }

public class StatusConditionManager : MonoBehaviour
{
    [SerializeField] private Image StatusIcon;
    public TMP_Text countdown;
    [SerializeField] private Sprite Counter,Pure,Block,Fire,Poison;

    public void UpdateStatusIcon(StatusConditions c){
        if(c == StatusConditions.NoCondition){
            gameObject.SetActive(false);
        }
        else{
            gameObject.SetActive(true);
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
        int n = UnityEngine.Random.Range(0,temp.Length);
        Debug.Log("Chose random condition " + n);
        return temp[n];
    }
}
