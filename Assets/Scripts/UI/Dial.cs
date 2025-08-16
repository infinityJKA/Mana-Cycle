using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Menus
{
    // UI component to select a number in a descrete range
    public class Dial : MonoBehaviour, IMoveHandler
    {
        // TODO make custom drawer
        [SerializeField] private TMPro.TMP_Text valueText;
        [SerializeField] private int min;
        [SerializeField] private int max;
        [SerializeField] private int baseValue;
        private int value;
        [SerializeField] private bool useValueNames = true;
        [SerializeField] private string[] valueNames;
        
        [SerializeField] private bool useXAxis = true;

        public delegate void OnValueChangedHandler(int value);
        public event OnValueChangedHandler OnValueChanged;

        public void Awake()
        {
            value = Math.Clamp(baseValue, min, max);
            valueText.text = useValueNames ? valueNames[value - min] : "" + value;
        }

        public void OnMove(AxisEventData eventData)
        {
            int delta = (int) (useXAxis ? eventData.moveVector.x : eventData.moveVector.y); 
            if (delta != 0)
            {
                value = Math.Clamp(value + delta, min, max);
                valueText.text = useValueNames ? valueNames[value] : "" + value;
            }
        }
    }
}