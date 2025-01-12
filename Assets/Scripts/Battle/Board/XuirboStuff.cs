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

    public GameObject menuGameObject;
    public TMP_Text menuText;

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
        inflationText.text = "Inflation: "+inflation;
    
    
    }

}
