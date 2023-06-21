using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SoloMode
{
    public class TabMoveSelectable : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private LevelLister levelLister;

        /// <summary>
        /// if false, tabs left and not right
        /// </summary>
        [SerializeField] private bool right;

        public void OnSelect(BaseEventData eventData)
        {
            StartCoroutine(TabAfterFrame());
        }

        IEnumerator TabAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            if (right)
            {
                levelLister.RightTabArrow();
            }
            else
            {
                levelLister.LeftTabArrow();
            }
        }
    }
}