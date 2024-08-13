using UnityEngine;

using Sound;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using Battle.Board;
using UnityEngine.UI;

namespace Battle {
    public class DamageShoot : MonoBehaviour {
        /** The amount of damage carried by this DamageShoot */
        public int damage {get; private set;}

        /** True if countering incoming damage; False if damaging an enemy */
        private Mode mode;
        public enum Mode {
            Standby, // basically just the default mode when this is intantated
            AddScore, // sent towards hp bar, used in singleplayer score-based modes
            Healing, // sent towards hp number, same as add score but caps at max hp, used in versus modes
            Countering, // sent towards an incoming damage on the sender's own board
            Shielding, // sent towards shield icon location to add shield
            AttackingShield, // sent towards opponent's shield icon; will damage their shield and send to queue after
            Attacking // sent towards opponent's incoming damage bar to add damage. will still damage shield if there is somehow any.
        }

        /** The board that this is targeting, for countering or dealing damage. */
        private GameBoard target;

        /** Position to move towards */
        private Vector3 destination;

        /** Time the shoot should take to reach wherever it is going */
        [SerializeField] private float travelTime = 0.4f;

        /** small additional speed boost in screenwidths/sec added to speed **/
        [SerializeField] private float additionalSpeed = 0.05f;

        private float speed;

        private float speedMultiplier;

        [SerializeField] private GameObject dmgShootSFX;

        // ==== VISUALS
        [SerializeField] private TrailRenderer trail;

        [SerializeField] private Image glowImage;
        [SerializeField] private Image manaImage;

        private Transform spawnedParticleSystem;


        [SerializeField] private int[] visualLevelThresholds;

        [SerializeField] private float[] sizes;

        [ColorUsage(true, true)]
        [SerializeField] private Color[] colors;

        [SerializeField] private Material[] glowMaterials;
        [SerializeField] private Material[] trailMaterials;

        [SerializeField] private GameObject[] levelParticles;
        [SerializeField] private float[] speedMultipliers;

        void Update() {
            if (mode == Mode.Standby) return;

            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.smoothDeltaTime);

            if (spawnedParticleSystem) {
                spawnedParticleSystem.transform.LookAt(destination);
            }

            // If this damageShoot has reached its destination, counter/damage
            if (ReachedDestination()) {
                EvaluateOnDestination();
            }
        }

        public void SetDamageAndVisuals(int damage, Sprite manaSprite) {
            this.damage = damage;

            int visualLevel = 0;
            while (damage >= visualLevelThresholds[visualLevel+1]) {
                visualLevel++;
                if (visualLevel == visualLevelThresholds.Length-1) break;
            }
            
            Color color = colors[visualLevel];
            manaImage.color = color;

            glowImage.material = glowMaterials[visualLevel];
            trail.material = trailMaterials[visualLevel];

            float size = sizes[visualLevel];
            glowImage.rectTransform.sizeDelta = new Vector2(size, size);
            manaImage.rectTransform.sizeDelta = new Vector2(size * 0.75f, size * 0.75f);
            manaImage.sprite = manaSprite;
            trail.startWidth = size * 0.6f;
            trail.endWidth = 0;

            speedMultiplier = speedMultipliers[visualLevel];
            
            var particles = levelParticles[visualLevel];
            if (particles) {
                spawnedParticleSystem = Instantiate(particles, transform).transform;
            }
        }

        public void SetDamage(int damage) {
            this.damage = damage;
        }

        public void Shoot(GameBoard target, Mode mode, Vector3 destination) {
            this.target = target;
            this.mode = mode;
            this.destination = destination; 
            
            // maintain current z plane to not get sent behind pieces
            this.destination.z = EffectCanvas.instance.transform.position.z;

            // in case this has already been shot and is now travelling towards its new target, reset to base unacellerated speed, 
            // will somewhat signify a momentum (damage) transfer
            speed = (destination - transform.position).magnitude / travelTime + additionalSpeed * Screen.width;
            speed *= speedMultiplier;

            Debug.Log("shooting towards "+target.name+" with mode "+mode);
        }

        void EvaluateOnDestination() {
            if (mode == Mode.Countering) {
                // Counter any incoming damage in the queue
                damage = target.hpBar.CounterIncoming(damage);

                // Stop here if no damage left
                if (damage == 0) {
                    Destroy(gameObject);
                    return;
                }

                // if Pyro, after all countering, send towards shield icon to add shield if shield is not already full
                if (target.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Shields && target.shield < target.abilityManager.PyroMaxDamageDealShield()) {
                    Shield(target);
                    return;
                } 
                // otherwise, attack the opponent
                else {
                    // undo romra defensive +30% damage boost
                    if (target.Battler.passiveAbilityEffect == Battler.PassiveAbilityEffect.Defender) {
                        damage = (int)(damage / 1.3f);
                    }
                    Attack(target, target.enemyBoard);
                }
            }

            else if (mode == Mode.Shielding) {
                int possibleShield = target.abilityManager.PyroMaxDamageDealShield() - target.shield;
                if (possibleShield > 0) {
                    // if there is more damage than allowed shield, fill all alotted shield and save leftover damage
                    if (damage > possibleShield) {
                        target.AddShield(possibleShield);
                        damage -= possibleShield;
                    } 
                    // otherwise, add it all to shield; this damageshoot stops here
                    else {
                        target.AddShield(damage);
                        Destroy(gameObject);
                    }
                }

                // if there is any damage left over after shielding (or no shielding happened at all), send to opponent
                if (damage > 0) {
                    Attack(target, target.enemyBoard);
                }
            }

            // default: deal damage to target
            else if (mode == Mode.AttackingShield) {
                damage = target.DamageShield(damage);
                if (damage > 0) {
                    // if leftover damage after attacking shield, send to their damage queue
                    Shoot(target, Mode.Attacking, target.hpBar.DamageQueue[0].transform.position);
                } else {
                    Destroy(gameObject);
                }
            }

            else if (mode == Mode.Attacking) {
                target.EnqueueDamage(damage);
                Destroy(gameObject);
            }

            else if (mode == Mode.AddScore) {
                target.AddScore(damage);
                Destroy(gameObject);
            }

            else if (mode == Mode.Healing) {
                target.Heal(damage);
                Destroy(gameObject);
            }
        }

        public void Shield(GameBoard target) {
            Shoot(target, Mode.Shielding, target.hpBar.shieldObject.transform.position);
        }

        /// <summary>
        /// Send this damage shoot towards the opponent, changing it to attack mode
        /// Should only be called if this damage shoot is not already in an attacking (so only defense or newly instantiated)
        /// </summary>
        /// <param name="target">the board being attacked</param>
        public void Attack(GameBoard attacker, GameBoard target) {
            // don't shoot at opponent while they are in recovery mode
            if (target.recoveryMode) {
                Destroy(gameObject);
                return;
            }

            if (mode == Mode.AttackingShield || mode == Mode.Attacking) {
                Debug.LogError("Trying to attack again with a damage shoot that has already been sent to attack");
                Destroy(gameObject);
                return;
            }

            attacker.matchStats.totalScore += damage;

            if (target.shield > 0) {
                Shoot(target, Mode.AttackingShield, target.hpBar.shieldObject.transform.position);
            } else {
                Shoot(target, Mode.Attacking, target.hpBar.DamageQueue[0].transform.position);
            }
        }

        /** Checks if this has reached its destination */
        bool ReachedDestination() {
            return (destination - transform.position).sqrMagnitude < 0.01f;
        }
    }
}