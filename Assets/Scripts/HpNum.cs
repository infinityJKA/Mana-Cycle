using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpNum : MonoBehaviour
{
    TMPro.TextMeshProUGUI textbox;
    void Start()
    {
        textbox = GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void SetHealth(int health)
    {
        if (health > 0)
        {
            textbox.enabled = true;
            textbox.text = health.ToString();
        } else {
            textbox.enabled = false;
        }
    }
}
