using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battle.Cycle;

namespace Battle.Board {
    public class Tile : MonoBehaviour
    {
        // Mana color enum value for this tile
        public ManaColor color { get; private set; }
        // Seperate image object attached to this
        public GameObject imageObject;
        // Target position of this element
        private Vector3 targetPosition;
        // Initial movement speed of this object when movmenet animated - distance in tiles per sec
        public float initialSpeed = 0;
        private float speed;
        // Acceleration of this piece when falling
        public float acceleration = 10; 
        // If this piece is currently moving
        private bool moving = false;

        public Action onFallAnimComplete;

        public void SetColor(ManaColor color, GameBoard board)
        {
            // Get image and set color from the list in this scene's cycle
            this.color = color;
            imageObject.GetComponent<Image>().color = board.cycle.GetManaColors()[ ((int)color) ];
            if (board.cycle.usingSprites) imageObject.GetComponent<Image>().sprite = board.cycle.manaSprites[ ((int)color) ];
        }

        public ManaColor GetManaColor()
        {
            return color;
        }

        public void AnimateMovement(Vector2 from, Vector2 to) {
            imageObject.transform.localPosition = from;
            targetPosition = to;
            speed = initialSpeed;
            moving = true;
        }

        private void Update() {
            if (moving) {
                if ((imageObject.transform.localPosition - targetPosition).sqrMagnitude < 0.005f) {
                    moving = false;
                    if (onFallAnimComplete != null) onFallAnimComplete();
                } else {
                    imageObject.transform.localPosition = Vector2.MoveTowards(imageObject.transform.localPosition, targetPosition, speed*Time.deltaTime);
                    speed += acceleration*Time.deltaTime;
                }
            }
        }
    }
}