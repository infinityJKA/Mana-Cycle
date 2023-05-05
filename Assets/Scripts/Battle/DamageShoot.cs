using UnityEngine;

namespace Battle {
    public class DamageShoot : MonoBehaviour {
        /** The amount of damage carried by this DamageShoot */
        public int damage;

        /** True if countering incoming damage; False if damaging an enemy */
        public bool countering;

        /** The board that this is targeting, for countering or dealing damage. */
        public Board.GameBoard target;

        /** Position to move towards */
        public Vector3 destination;

        /** speed, in screen widths / sec */
        public float speed = 1f;

        /** acceleration, in screen widths / sec / sec **/
        public float accel = 1f;

        // save initial un-accelerated speed to reset back to if countering
        private float initialSpeed;
        void Start() {
            initialSpeed = speed;
        }

        void Update() {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Screen.width * Time.deltaTime);
            speed += accel * Time.deltaTime;

            // If this damageShoot has reached its destination, counter/damage
            if (ReachedDestination()) {
                if (countering) {
                    // Counter incoming
                    // ive just started making fields public idc at this point
                    int residualDamage = target.hpBar.CounterIncoming(damage);

                    // If there is any leftover damage, travel to enemy board and damage them
                    if (residualDamage > 0) {
                        damage = residualDamage;
                        destination = target.enemyBoard.hpBar.DamageQueue[0].transform.position;
                        countering = false;
                        speed = initialSpeed;
                    } else {
                        // if not, destroy here
                        Destroy(this.gameObject);
                    }
                }

                else { 
                    // if singleplayer, add to "score" (hp bar)
                    if (target.singlePlayer && !Storage.level.aiBattle) {
                        target.AddScore(damage);
                    } 
                    // else deal damage to target
                    else {
                        target.EnqueueDamage(damage);
                    }
                    Destroy(this.gameObject);
                }
            }
        }

        /** Checks if this has reached its destination */
        bool ReachedDestination() {
            return (destination - transform.position).sqrMagnitude < 0.01f;
        }
    }
}