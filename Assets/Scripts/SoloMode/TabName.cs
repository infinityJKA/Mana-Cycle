using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SoloMode
{
    public class TabName : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector] public LevelLister levelLister;

        [HideInInspector] public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            levelLister.SetTabCursor(index);
        }
    }
}