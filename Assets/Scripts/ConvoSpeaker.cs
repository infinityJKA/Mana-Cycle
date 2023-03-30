using UnityEngine;
using UnityEngine.UI;

public class ConvoSpeaker : MonoBehaviour {
    /** Battler being displayed as speaker */
    [SerializeField] private Battler speaker;
    /** Image sprite of speaker */
    [SerializeField] private Image portrait;
    /** Gameobject for speaker's name */
    [SerializeField] private GameObject nameObj;
    /** Text GUI for speaker's name */
    [SerializeField] private TMPro.TextMeshProUGUI nameGUI;

    public void SetSpeaker(Battler speaker, bool focused) {
        this.speaker = speaker;

        if (speaker != null) {
            gameObject.SetActive(true);
            nameObj.SetActive(true);
            portrait.sprite = speaker.sprite;
            portrait.color = new Color(1.0f, 1.0f, 1.0f, focused ? 1.0f : 0.5f);
            nameGUI.text = speaker.name;
        } 
        
        else {
            gameObject.SetActive(false);
            nameObj.SetActive(false);
        }
    }
}