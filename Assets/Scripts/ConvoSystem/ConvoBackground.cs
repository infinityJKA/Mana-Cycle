using UnityEngine;
using UnityEngine.EventSystems;

namespace ConvoSystem {
    public class ConvoBackground : MonoBehaviour /**, IPointerClickHandler **/ {
        [SerializeField] private ConvoHandler convoHandler;

        void Update() {
            // TODO fix
            if (Input.GetMouseButtonDown(0)) {
                convoHandler.Advance();
            }
        }

        // public void OnPointerClick(PointerEventData eventData)
        // {
        //     // Debug.Log(eventData);
        //     convoHandler.Advance();
        // }
    }
}