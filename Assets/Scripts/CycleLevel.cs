using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleLevel : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI label;

    public void Set(int label)
    {
        this.label.text = label.ToString();
    }
}
