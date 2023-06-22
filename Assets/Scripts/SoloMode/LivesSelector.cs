using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using Sound;
using VersusMode;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace MainMenu {
    /// <summary>
    /// Controls the cinemachine camera in the 3d menu.
    /// <summary>
    public class LivesSelector : MonoBehaviour, IMoveHandler
    {
        [SerializeField] private CharSelector selector;

        public void OnMove(AxisEventData eventData)
        {
            Debug.Log(eventData);

            if (eventData.moveDir == MoveDirection.Left)
            {
                selector.SettingsCursorLeft();
            }

            else if (eventData.moveDir == MoveDirection.Right)
            {
                selector.SettingsCursorRight();
            }
        }
    }
}