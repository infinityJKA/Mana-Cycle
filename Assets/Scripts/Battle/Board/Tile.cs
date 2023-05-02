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
        public Image image;
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


        // If this is a trash tile - which damages in set intervals
        public bool trashTile { get; private set; }

        public void SetColor(ManaColor color, GameBoard board)
        {
            // Get image and set color from the list in this scene's cycle
            this.color = color;
            image.color = board.cycle.GetManaColors()[ ((int)color) ];
            if (board.cycle.usingSprites) image.sprite = board.cycle.manaSprites[ ((int)color) ];
        }

        public ManaColor GetManaColor()
        {
            return color;
        }

        public void SetVisualColor(Color color)
        {
            image.color = color;
        }

        public void AnimateMovement(Vector2 from, Vector2 to) {
            image.transform.localPosition = from;
            targetPosition = to;
            speed = initialSpeed;
            moving = true;
        }

        private void Update() {
            if (moving) {
                if (image.transform.localPosition == targetPosition) {
                    moving = false;
                    if (onFallAnimComplete != null) onFallAnimComplete();
                } else {
                    image.transform.localPosition = Vector2.MoveTowards(image.transform.localPosition, targetPosition, speed*Time.deltaTime);
                    speed += acceleration*Time.deltaTime;
                }
            }
        }

        public void MakeTrashTile(GameBoard board) {
            trashTile = true;
            SetVisualColor(Color.Lerp(Color.black, image.color, 0.7f));
        }
    }
}
