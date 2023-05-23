using UnityEngine;

using Sound;

namespace Battle {
    public class DamageShoot : MonoBehaviour {
        /** The amount of damage carried by this DamageShoot */
        public int damage;

        /** True if countering incoming damage; False if damaging an enemy */
        public Mode mode;
        public enum Mode {
            Attacking,
            Countering,
            Shielding,
            AttackingEnemyShield,
            Heal // add score in solo mode
        }

        /** The board that this is targeting, for countering or dealing damage. */
        public Board.GameBoard target;

        /** Position to move towards */
        public Vector3 destination;

        /** speed, in screen widths / sec */
        public float speed = 1f;

        /** acceleration, in screen widths / sec / sec **/
        public float accel = 1f;

        [SerializeField] private AudioClip dealDmgSFX;

        // save initial un-accelerated speed to reset back to if countering
        private float initialSpeed;
        void Start() {
            initialSpeed = speed;
        }

        void Update() {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Screen.width * Time.smoothDeltaTime);
            speed += accel * Time.smoothDeltaTime;

            // If this damageShoot has reached its destination, counter/damage
            if (ReachedDestination()) {
                if (mode == Mode.Countering) {
                    // Counter incoming
                    // ive just started making fields public idc at this point
                    int residualDamage = target.hpBar.CounterIncoming(damage);

                    // If there is any leftover damage, travel to enemy board and damage them
                    if (residualDamage > 0) {
                        damage = residualDamage;
                        Attack(target.enemyBoard);
                    } else {
                        // if not, destroy here
                        Destroy(this.gameObject);
                    }
                }

                else if (mode == Mode.Shielding) {
                    int overshield = target.AddShield(damage);

                    // If there is any leftover damage, travel to enemy board and damage them
                    if (overshield > 0) {
                        damage = overshield;
                        Attack(target.enemyBoard);
                    } else {
                        // if not, destroy here
                        Destroy(this.gameObject);
                    }
                }

                else if (mode == Mode.AttackingEnemyShield) {
                    int overflow = target.DamageShield(damage);

                    // If there is any leftover damage, travel to enemy board and damage them
                    if (overflow > 0) {
                        damage = overflow;
                        Attack(target);
                    } else {
                        // if not, destroy here
                        Destroy(this.gameObject);
                    }
                }

                else if (mode == Mode.Heal) { 
                    // if singleplayer, add to "score" (hp bar)
                    if (target.singlePlayer && !Storage.level.aiBattle) {
                        target.AddScore(damage);
                        Destroy(this.gameObject);
                    }
                }

                // default: deal damage to target
                else {
                    // target is invincible while in recovery mode
                    if (target.recoveryMode) {
                        Destroy(this.gameObject); 
                        return;
                    }

                    target.EnqueueDamage(damage);
                    Destroy(this.gameObject);
                    target.PlaySFX("dmgShoot", pitch: 1f + target.hpBar.DamageQueue[0].dmg/1000f, volumeScale: 1.5f);
                }
            }
        }

        void Attack(Battle.Board.GameBoard target) {
            this.target = target;

            if (target.shield > 0) {
                mode = Mode.AttackingEnemyShield;
                destination = target.hpBar.shieldObject.transform.position;
            } else {
                mode = Mode.Attacking;
                destination = target.enemyBoard.hpBar.DamageQueue[0].transform.position;
            }

            speed = initialSpeed;
        }

        /** Checks if this has reached its destination */
        bool ReachedDestination() {
            return (destination - transform.position).sqrMagnitude < 0.01f;
        }
    }
}