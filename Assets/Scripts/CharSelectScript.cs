using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CharSelectScript : MonoBehaviour
{
    [SerializeField] private InputScript inputScript;
    [SerializeField] private List<Battler> battlerList;
    [SerializeField] private GameObject portraitDisp;
    [SerializeField] private GameObject nameDisp;
    private TMPro.TextMeshProUGUI nameText;
    private Image portraitImg;

    private int charSelection = 0;

    // Start is called before the first frame update
    void Start()
    {
        nameText = nameDisp.GetComponent<TMPro.TextMeshProUGUI>();
        portraitImg = portraitDisp.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(inputScript.Left))
        {
            charSelection--;
        }

        if (Input.GetKeyDown(inputScript.Right))
        {
            charSelection++;
        }

        // keep selection in bounds
        charSelection = Math.Abs(charSelection) % battlerList.Count;

        // update objects
        Battler currentChar = battlerList[charSelection];
        nameText.text = currentChar.displayName;
        portraitImg.sprite = currentChar.sprite;
    }
}
