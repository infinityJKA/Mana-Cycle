using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    // The board this HP bar is for
    private GameBoard board;

    [SerializeField] public HpNum hpNum;
    [SerializeField] private GameObject IncomingDmgDisp;

    private Image hpBarImage;
    private Image incomingDmgBarImage;
    private RectTransform hpBarRectTransform;
    private RectTransform incomingDmgBarRectTransform;

    // using floats instead of ints to get percentage
    // just guessing max hp based on dmg formula, also might be different per character later on
    private float hpBarTopY;
    [SerializeField] private IncomingDamage[] damageQueue;

    public IncomingDamage[] DamageQueue { get { return damageQueue; }}

    // Start is called before the first frame update
    void Start()
    {   
        // get image components to edit attributes 
        hpBarImage = hpNum.GetComponent<Image>();
        incomingDmgBarImage = IncomingDmgDisp.GetComponent<Image>();

        // get rect transform to change positions later
        hpBarRectTransform = hpNum.GetComponent<RectTransform>();
        incomingDmgBarRectTransform = IncomingDmgDisp.GetComponent<RectTransform>();

        foreach (IncomingDamage incoming in damageQueue)
        {
            incoming.SetDamage(0);
        }
    }

    // Initializes this HP bar for the GameBoard passed in which this hp bar is for.
    void Setup(GameBoard board)
    {
        this.board = board;
    }

    // Update is called once per frame
    void Update()
    {
        // percentage of hpbar and dmgbar that should be filled
        // hpBarImage.fillAmount = currentHp / maxHp;
        // incomingDmgBarImage.fillAmount = incomingDmg / maxHp;
        
        // placement of dmg indicator, top of current hp bar
        // hpBarTopY = hpBarImage.fillAmount * hpBarRectTransform.localScale.y - hpBarRectTransform.localScale.y + 0.5f; // idk why you need the 0.5 in there i just adjusted it until it worked (lol)
        // change anchors of bar to update position

        // TODO: Jack - disabled these 3 lines because erroring - morgan plz fix it idk how this code works
        // incomingDmgBarRectTransform.anchorMin = new Vector2(incomingDmgBarRectTransform.anchorMin.x, hpBarTopY);
        // incomingDmgBarRectTransform.anchorMax = new Vector2(incomingDmgBarRectTransform.anchorMax.x, hpBarTopY);
        // incomingDmgBarRectTransform.anchoredPosition = Vector2.zero;

        // Debug.Log(hpBarTopY);
    }

    public void SetHealth(int health)
    {
        hpNum.SetHealth(health);
    }

    public void AdvanceDamageQueue()
    {
        // Advance the incoming damage cycle
        for (int i = 5; i >= 1; i--)
        {
            damageQueue[i].SetDamage(damageQueue[i-1].dmg);
            
        }

        damageQueue[0].SetDamage(0);
    }
}
