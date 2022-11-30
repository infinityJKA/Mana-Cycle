using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] private GameObject HpDisp;
    [SerializeField] private GameObject IncomingDmgDisp;
    private Image hpBarImage;
    private Image incomingDmgBarImage;
    private RectTransform hpBarRectTransform;
    private RectTransform incomingDmgBarRectTransform;
    // using floats instead of ints to get percentage 
    private float currentHp;
    // just guessing max hp based on dmg formula, also might be different per character later on
    public float maxHp = 1000f;
    private float incomingDmg;
    private float hpBarTopY;

    // Start is called before the first frame update
    void Start()
    {   
        currentHp = maxHp;
        incomingDmg = 0;

        // get image components to edit attributes 
        hpBarImage = HpDisp.GetComponent<Image>();
        incomingDmgBarImage = IncomingDmgDisp.GetComponent<Image>();

        // get rect transform to change positions later
        hpBarRectTransform = HpDisp.GetComponent<RectTransform>();
        incomingDmgBarRectTransform = IncomingDmgDisp.GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {

        // percentage of hpbar and dmgbar that should be filled
        hpBarImage.fillAmount = currentHp / maxHp;
        incomingDmgBarImage.fillAmount = incomingDmg / maxHp;
        
        // placement of dmg indicator, top of current hp bar
        hpBarTopY = hpBarImage.fillAmount * hpBarRectTransform.localScale.y - hpBarRectTransform.localScale.y + 0.5f; // idk why you need the 0.5 in there i just adjusted it until it worked (lol)
        // change anchors of bar to update position
        incomingDmgBarRectTransform.anchorMin = new Vector2(incomingDmgBarRectTransform.anchorMin.x, hpBarTopY);
        incomingDmgBarRectTransform.anchorMax = new Vector2(incomingDmgBarRectTransform.anchorMax.x, hpBarTopY);
        incomingDmgBarRectTransform.anchoredPosition = Vector2.zero;
        // Debug.Log(hpBarTopY);
    }
}
