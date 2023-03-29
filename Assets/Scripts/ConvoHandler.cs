using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConvoHandler : MonoBehaviour
{
    [SerializeField] private InputScript inputScript;
    [SerializeField] private Conversation[] convos;
    [SerializeField] private GameObject convoUI;
    private GameObject s1Portrait;
    private GameObject s2Portrait;
    private Image s1Img;
    private Image s2Img;
    private GameObject s1NameBox;
    private GameObject s2NameBox;
    private TMPro.TextMeshProUGUI s1NameText;
    private TMPro.TextMeshProUGUI s2NameText;
    private TMPro.TextMeshProUGUI dialougeText;

    private int convoPos;
    private Conversation currentConvo;

    // start doesn't work here because all of these objects are set as not active on start
    void InitVars()
    {
        s1Portrait = GameObject.Find("Speaker1");
        s2Portrait = GameObject.Find("Speaker2");
        s1Img = s1Portrait.GetComponent<Image>();
        s2Img = s2Portrait.GetComponent<Image>();

        s1NameBox = GameObject.Find("SpeakerNameBox1");
        s2NameBox = GameObject.Find("SpeakerNameBox2");
        s1NameText = GameObject.Find("SpeakerNameText1").GetComponent<TMPro.TextMeshProUGUI>();
        s2NameText = GameObject.Find("SpeakerNameText2").GetComponent<TMPro.TextMeshProUGUI>();

        dialougeText = GameObject.Find("DialougeText").GetComponent<TMPro.TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!convoUI.activeSelf) return;

        if (Input.GetKeyDown(inputScript.Cast))
        {
            convoPos++;
            if (convoPos >= currentConvo.dialougeList.Length){
                EndConvo();
            }
            else{
                RefreshObjects();
            }
        }
    }

    public void StartConvo(int convoIndex)
    {
        convoPos = 0;
        currentConvo = convos[convoIndex];
        convoUI.SetActive(true);
        InitVars();
        RefreshObjects();
    }

    void EndConvo()
    {
        // convoUI.SetActive(false);
        if (! (currentConvo.endScene == ""))
        {
            GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene(currentConvo.endScene);
        }
        else
        {
            convoUI.SetActive(false);
        }
        
    }

    void RefreshObjects()
    {
        dialougeText.text = currentConvo.dialougeList[convoPos];

        Battler lSpeaker = currentConvo.lSpeakerOrder[convoPos];
        Battler rSpeaker = currentConvo.rSpeakerOrder[convoPos];
        if (!(lSpeaker == null)){
            s1Portrait.SetActive(true);
            s1NameBox.SetActive(true);
            s1Img.sprite = lSpeaker.sprite;
            s1NameText.text = lSpeaker.displayName;
        }
        else{
            // narrator
            s1Portrait.SetActive(false);
            s1NameBox.SetActive(false);
        }

        if (!(rSpeaker == null)){
            s2Portrait.SetActive(true);
            s2NameBox.SetActive(true);
            s2Img.sprite = rSpeaker.sprite;
            s2NameText.text = rSpeaker.displayName;
        }
        else{
            // narrator
            s2Portrait.SetActive(false);
            s2NameBox.SetActive(false);
        }
        
        

        if (currentConvo.leftFocused[convoPos])
        {
            s1Img.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            s2Img.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        }
        else
        {
            s1Img.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            s2Img.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        }
        
    }

}
