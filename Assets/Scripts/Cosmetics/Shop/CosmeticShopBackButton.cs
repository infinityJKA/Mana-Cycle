using UnityEngine;
using UnityEngine.EventSystems;

namespace Cosmetics {
    public class CosmeticShopBackButton : MonoBehaviour, IMoveHandler {
        [SerializeField] private CosmeticShopTab tab;

        public void OnMove(AxisEventData eventData)
            {
                if (eventData.moveDir == MoveDirection.Left && tab.lastSelected) {
                    tab.lastSelected.Select();
                }
            }
    }
}