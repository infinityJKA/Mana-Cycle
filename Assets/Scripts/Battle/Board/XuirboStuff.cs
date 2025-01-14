using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class XuirboStuff : MonoBehaviour
{
    public int money,bait,crimes;
    public float inflation;
    public int circleStock,circlePrice,triangleUpStock,triangleUpPrice,triangleDownStock,triangleDownPrice,squareStock,squarePrice,diamondStock,diamondPrice;

    [SerializeField] TMP_Text moneyText,baitText,crimesText,inflationText,
    cStockText,cPriceText,tuStockText,tuPriceText,tdStockText,tdPriceText,sStockText,sPriceText,dStockText,dPriceText;

    public GameObject menuGameObject,badPopupGameObject,fishingPopupGameObject,mailPopupGameObject;
    public TMP_Text menuText,badText,fishingText,mailText;


    public float inflationTimer,stockTimer1,stockTimer2,stockTimer3,stockTimer4,stockTimer5,badPopupTimer;

    public void XuirboUpdate(){
        if(badPopupGameObject.activeSelf){
            if(badPopupTimer + 3 <= Time.time){
                badPopupGameObject.SetActive(false);
            }
        }

        if(inflationTimer + 0.5 <= Time.time){
            inflation = inflation*0.999f;
            
            inflationTimer = Time.time;
            UpdateXuirboText();
        }

        if(stockTimer1 + 5 <= Time.time){
            circleStock = (int)(circleStock*0.99f);
            circlePrice = (int)(circlePrice*0.99f);
            
            stockTimer1 = Time.time + UnityEngine.Random.Range(-1f, 1f);
            UpdateXuirboText();
        }

        if(stockTimer2 + 5 <= Time.time){
            triangleUpStock = (int)(triangleUpStock*0.99f);
            triangleUpPrice = (int)(triangleUpPrice*0.99f);
            
            stockTimer2 = Time.time + UnityEngine.Random.Range(-1f, 1f);
            UpdateXuirboText();
        }

        if(stockTimer3 + 5 <= Time.time){
            triangleDownStock = (int)(triangleDownStock*0.99f);
            triangleDownPrice = (int)(triangleDownPrice*0.99f);
            
            stockTimer3 = Time.time + UnityEngine.Random.Range(-1f, 1f);
            UpdateXuirboText();
        }

        if(stockTimer4 + 5 <= Time.time){
            squareStock = (int)(squareStock*0.99f);
            squarePrice = (int)(squarePrice*0.99f);
            
            stockTimer4 = Time.time + UnityEngine.Random.Range(-1f, 1f);
            UpdateXuirboText();
        }

        if(stockTimer5 + 5 <= Time.time){
            diamondStock = (int)(diamondStock*0.99f);
            diamondPrice = (int)(diamondPrice*0.99f);
            
            stockTimer5 = Time.time + UnityEngine.Random.Range(-1f, 1f);
            UpdateXuirboText();
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

        inflation = MathF.Round(inflation,4);
        inflationText.text = "Inflation: "+inflation;
    
    
    }

}
