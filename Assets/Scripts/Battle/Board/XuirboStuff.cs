using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Board;
using TMPro;
using UnityEngine;

public class XuirboStuff : MonoBehaviour
{
    public int money,bait,crimes, mercenaries, miners, flesh;
    public float inflation;
    public int circleStock,circlePrice,triangleUpStock,triangleUpPrice,triangleDownStock,triangleDownPrice,squareStock,squarePrice,diamondStock,diamondPrice;

    public bool policeActive;
    
    [SerializeField] TMP_Text moneyText,baitText,crimesText,inflationText,
    cStockText,cPriceText,tuStockText,tuPriceText,tdStockText,tdPriceText,sStockText,sPriceText,dStockText,dPriceText,
    fleshText,mercenariesText,minersText,actionCountdownText,policeText;

    public GameObject menuGameObject,badPopupGameObject,fishingPopupGameObject,mailPopupGameObject, policePopup, fleshPopup;
    public TMP_Text menuText,badText,fishingText,mailText;
    [SerializeField] GameBoard board; 


    public float inflationTimer,stockTimer1,stockTimer2,stockTimer3,stockTimer4,stockTimer5,badPopupTimer,actionTimer,policeTimer,fishingTimer,fleshTimer;

    public void XuirboUpdate(){
        if(badPopupGameObject.activeSelf){
            if(badPopupTimer + 3 <= Time.time){
                badPopupGameObject.SetActive(false);
            }
        }

        if(fleshPopup.activeSelf){
            if(fleshTimer + 6 <= Time.time){
                board.enemyBoard.SetHp(-9999);
                fleshPopup.SetActive(false);
            }
        }

        if(fishingPopupGameObject.activeSelf){
            if(fishingTimer + 3 <= Time.time){
                fishingPopupGameObject.SetActive(false);
            }
        }

        if(inflationTimer + 0.5 <= Time.time){
            inflation = inflation*0.999f;
            
            inflationTimer = Time.time;
        }

        if(stockTimer1 + 5 <= Time.time){
            circleStock = (int)(circleStock*0.99f);
            circlePrice = (int)(circlePrice*0.99f);
            
            stockTimer1 = Time.time + UnityEngine.Random.Range(-1f, 1f);
        }

        if(stockTimer2 + 5 <= Time.time){
            triangleUpStock = (int)(triangleUpStock*0.99f);
            triangleUpPrice = (int)(triangleUpPrice*0.99f);
            
            stockTimer2 = Time.time + UnityEngine.Random.Range(-1f, 1f);
        }

        if(stockTimer3 + 5 <= Time.time){
            triangleDownStock = (int)(triangleDownStock*0.99f);
            triangleDownPrice = (int)(triangleDownPrice*0.99f);
            
            stockTimer3 = Time.time + UnityEngine.Random.Range(-1f, 1f);
        }

        if(stockTimer4 + 5 <= Time.time){
            squareStock = (int)(squareStock*0.99f);
            squarePrice = (int)(squarePrice*0.99f);
            
            stockTimer4 = Time.time + UnityEngine.Random.Range(-1f, 1f);
        }

        if(stockTimer5 + 5 <= Time.time){
            diamondStock = (int)(diamondStock*0.99f);
            diamondPrice = (int)(diamondPrice*0.99f);
            
            stockTimer5 = Time.time + UnityEngine.Random.Range(-1f, 1f);
        }

        if(actionTimer + 7 <= Time.time){
            board.EvaluateInstantOutgoingDamage(mercenaries*33);
            money += miners*((int)(40*inflation));
            board.hpBar.AdvanceDamageQueue();
            actionTimer = Time.time;
        }
        else{
            actionCountdownText.text = ""+(int)(actionTimer+7-Time.time+1);
        }

        if(policeActive){
            if(policeTimer + 30 <= Time.time){
                board.EvaluateInstantIncomingDamage(crimes*450);
                crimes = 0;
                Instantiate(board.cosmetics.policeSFX);
                policePopup.SetActive(false);
                policeActive = false;
            }
            else{
                policeText.text = "POLICE COMING IN "+(int)(policeTimer+30-Time.time+1)+" SECONDS";
            }
        }

        UpdateXuirboText();

    }

    public void CrimeChance(){
        crimes += 1;
        if(crimes > 1){
            if(!policeActive && UnityEngine.Random.Range(0,100) <= 33){
                policeTimer = Time.time;
                Instantiate(board.cosmetics.policeSFX);
                policeActive = true;
                policePopup.SetActive(true);
            }
        }
    }

    public void UpdateXuirboText(){
        // gaming
        cStockText.text = ""+circleStock;
        cPriceText.text = ""+circlePrice;
        tuStockText.text = ""+triangleUpStock;
        tuPriceText.text = ""+triangleUpPrice;
        tdStockText.text = ""+triangleDownStock;
        tdPriceText.text = ""+triangleDownPrice;
        sStockText.text = ""+squareStock;
        sPriceText.text = ""+squarePrice;
        dStockText.text = ""+diamondStock;
        dPriceText.text = ""+diamondPrice;

        moneyText.text = "$"+money;
        baitText.text = "Bait: "+bait;
        crimesText.text = "Crimes: "+crimes;
        mercenariesText.text = "Mercenaries: "+mercenaries;
        minersText.text = "Miners: "+miners;
        fleshText.text = "Flesh: "+flesh;

        inflation = MathF.Round(inflation,4);
        inflationText.text = "Inflation: "+inflation;
        
    
    
    }

}
