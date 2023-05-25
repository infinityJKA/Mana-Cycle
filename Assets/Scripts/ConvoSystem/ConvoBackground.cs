using UnityEngine;
using UnityEngine.EventSystems;

namespace ConvoSystem {
    public class ConvoBackground : MonoBehaviour, IPointerClickHandler {
        [SerializeField] private ConvoHandler convoHandler;

        public void OnPointerClick(PointerEventData eventData)
        {
            // Debug.Log(eventData);
            convoHandler.Advance();
        }
    }
}