using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingDamage : MonoBehaviour
{
    public int dmg { get; private set; }
    public TMPro.TextMeshProUGUI textComponent;

    void Start()
    {
        textComponent = GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void SetDamage(int damage)
    {
        dmg = damage;
        if (dmg > 0)
        {
            textComponent.enabled = true;
            textComponent.text = damage.ToString();
        } else {
            textComponent.enabled = false;
        }
    }

    public void AddDamage(int damage)
    {
        SetDamage(dmg + damage);
    }
}
